using AE.Net.Mail.Imap;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AE.Net.Mail {

	public class ImapClient : TextClient, IMailClient {
		private string _SelectedMailbox;
		private int _tag = 0;
		private string[] _Capability;

		private bool _Idling;
		private Thread _IdleEvents;

		private string _FetchHeaders = null;

		public ImapClient() { }
		public ImapClient(string host, string username, string password, AuthMethods method = AuthMethods.Login, int port = 143, bool secure = false, bool skipSslValidation = false) {
			Connect(host, port, secure, skipSslValidation);
			AuthMethod = method;
			Login(username, password);
		}

		public enum AuthMethods {
			Login,
			CRAMMD5,
			SaslOAuth
		}

		public virtual AuthMethods AuthMethod { get; set; }

		private string GetTag() {
			_tag++;
			return string.Format("xm{0:000} ", _tag);
		}

		public virtual bool Supports(string command) {
			return (_Capability ?? Capability()).Contains(command, StringComparer.OrdinalIgnoreCase);
		}

		private EventHandler<MessageEventArgs> _NewMessage;
		public virtual event EventHandler<MessageEventArgs> NewMessage {
			add {
				_NewMessage += value;
				IdleStart();
			}
			remove {
				_NewMessage -= value;
				if (!HasEvents)
					IdleStop();
			}
		}

		private EventHandler<MessageEventArgs> _MessageDeleted;
		public virtual event EventHandler<MessageEventArgs> MessageDeleted {
			add {
				_MessageDeleted += value;
				IdleStart();
			}
			remove {
				_MessageDeleted -= value;
				if (!HasEvents)
					IdleStop();
			}
		}

		protected virtual void IdleStart() {
			if (string.IsNullOrEmpty(_SelectedMailbox)) {
				SelectMailbox("Inbox");
			}
			_Idling = true;
			if (!Supports("IDLE")) {
				throw new InvalidOperationException("This IMAP server does not support the IDLE command");
			}
			CheckMailboxSelected();
			IdleResume();
		}

		protected virtual void IdlePause() {
			if (_IdleEvents == null || !_Idling)
				return;

			CheckConnectionStatus();
			SendCommand("DONE");
			if (!_IdleEvents.Join(2000))
				_IdleEvents.Abort();
			_IdleEvents = null;
		}

		protected virtual void IdleResume() {
			if (!_Idling)
				return;

			IdleResumeCommand();

			if (_IdleEvents == null) {
				_IdleEvents = new Thread(WatchIdleQueue);
				_IdleEvents.Name = "_IdleEvents";
				_IdleEvents.Start();
			}
		}

		private void IdleResumeCommand() {
			SendCommandGetResponse(GetTag() + "IDLE");
			_IdleARE.Set();
		}

		private bool HasEvents {
			get {
				return _MessageDeleted != null || _NewMessage != null;
			}
		}

		protected virtual void IdleStop() {
			_Idling = false;
			IdlePause();
			if (_IdleEvents != null) {
				_IdleARE.Close();
				if (!_IdleEvents.Join(2000))
					_IdleEvents.Abort();
				_IdleEvents = null;
			}
		}

		public virtual bool TryGetResponse(out string response, int millisecondsTimeout) {
			var mre = new System.Threading.ManualResetEventSlim(false);
			string resp = response = null;
			ThreadPool.QueueUserWorkItem(_ => {
				resp = GetResponse();
				mre.Set();
			});

			if (mre.Wait(millisecondsTimeout)) {
				response = resp;
				return true;
			} else
				return false;
		}

		private static readonly int idleTimeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
		private static AutoResetEvent _IdleARE = new AutoResetEvent(false);
		private void WatchIdleQueue() {
			try {
				string last = null, resp;

				while (true) {
					if (!TryGetResponse(out resp, idleTimeout)) {   //send NOOP every 20 minutes
						Noop(false);        //call noop without aborting this Idle thread
						continue;
					}

					if (resp.Contains("OK IDLE"))
						return;

					var data = resp.Split(' ');
					if (data[0] == "*" && data.Length >= 3) {
						var e = new MessageEventArgs { Client = this, MessageCount = int.Parse(data[1]) };
						if (data[2].Is("EXISTS") && !last.Is("EXPUNGE") && e.MessageCount > 0) {
							ThreadPool.QueueUserWorkItem(callback => _NewMessage.Fire(this, e));    //Fire the event on a separate thread
						} else if (data[2].Is("EXPUNGE")) {
							_MessageDeleted.Fire(this, e);
						}
						last = data[2];
					}
				}
			} catch (Exception) { }
		}

		protected override void OnDispose() {
			base.OnDispose();
			if (_IdleEvents != null) {
				_IdleEvents.Abort();
				_IdleEvents = null;
			}
		}

		public virtual void AppendMail(MailMessage email, string mailbox = null) {
			IdlePause();

			mailbox = ModifiedUtf7Encoding.Encode(mailbox);
			string flags = String.Empty;
			var body = new StringBuilder();
			using (var txt = new System.IO.StringWriter(body))
				email.Save(txt);

			string size = body.Length.ToString();
			if (email.RawFlags.Length > 0) {
				flags = " (" + string.Join(" ", email.Flags) + ")";
			}

			if (mailbox == null)
				CheckMailboxSelected();
			mailbox = mailbox ?? _SelectedMailbox;

			string command = GetTag() + "APPEND " + (mailbox ?? _SelectedMailbox).QuoteString() + flags + " {" + size + "}";
			string response = SendCommandGetResponse(command);
			if (response.StartsWith("+")) {
				response = SendCommandGetResponse(body.ToString());
			}
			IdleResume();
		}

		public virtual void Noop() {
			Noop(true);
		}
		private void Noop(bool pauseIdle) {
			if (pauseIdle)
				IdlePause();
			else
				SendCommandGetResponse("DONE");

			var tag = GetTag();
			var response = SendCommandGetResponse(tag + "NOOP");
			while (!response.StartsWith(tag)) {
				response = GetResponse();
			}

			if (pauseIdle)
				IdleResume();
			else
				IdleResumeCommand();
		}

		public virtual string[] Capability() {
			IdlePause();
			string command = GetTag() + "CAPABILITY";
			string response = SendCommandGetResponse(command);
			if (response.StartsWith("* CAPABILITY "))
				response = response.Substring(13);
			_Capability = response.Trim().Split(' ');
			GetResponse();
			IdleResume();
			return _Capability;
		}

		public virtual void Copy(string messageset, string destination) {
			CheckMailboxSelected();
			IdlePause();
			string prefix = null;
			if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
				messageset = messageset.Substring(4);
				prefix = "UID ";
			}
			string command = string.Concat(GetTag(), prefix, "COPY ", messageset, " " + destination.QuoteString());
			SendCommandCheckOK(command);
			IdleResume();
		}

		public virtual void CreateMailbox(string mailbox) {
			IdlePause();
			string command = GetTag() + "CREATE " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			SendCommandCheckOK(command);
			IdleResume();
		}

		public virtual void DeleteMailbox(string mailbox) {
			IdlePause();
			string command = GetTag() + "DELETE " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			SendCommandCheckOK(command);
			IdleResume();
		}

		public virtual Mailbox Examine(string mailbox) {
			IdlePause();

			Mailbox x = null;
			string tag = GetTag();
			string command = tag + "EXAMINE " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			string response = SendCommandGetResponse(command);
			if (response.StartsWith("*")) {
				x = new Mailbox(mailbox);
				while (response.StartsWith("*")) {
					Match m;
					m = Regex.Match(response, @"(\d+) EXISTS");
					if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
					m = Regex.Match(response, @"(\d+) RECENT");
					if (m.Groups.Count > 1)
						x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
					m = Regex.Match(response, @"UNSEEN (\d+)");
					if (m.Groups.Count > 1)
						x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
					m = Regex.Match(response, @" FLAGS \((.*?)\)");
					if (m.Groups.Count > 1)
						x.SetFlags(m.Groups[1].ToString());
					response = GetResponse();
				}
				_SelectedMailbox = mailbox;
			}
			IdleResume();
			return x;
		}

		public virtual void Expunge() {
			CheckMailboxSelected();
			IdlePause();

			string tag = GetTag();
			string command = tag + "EXPUNGE";
			string response = SendCommandGetResponse(command);
			while (response.StartsWith("*")) {
				response = GetResponse();
			}
			IdleResume();
		}

		public virtual void DeleteMessage(AE.Net.Mail.MailMessage msg) {
			DeleteMessage(msg.Uid);
		}

		public virtual void DeleteMessage(string uid) {
			CheckMailboxSelected();
			Store("UID " + uid, true, "\\Seen \\Deleted");
		}

		public virtual void MoveMessage(string uid, string folderName) {
			CheckMailboxSelected();
			Copy("UID " + uid, folderName);
			DeleteMessage(uid);
		}

		protected virtual void CheckMailboxSelected() {
			if (string.IsNullOrEmpty(_SelectedMailbox))
				SelectMailbox("INBOX");
		}

		public virtual MailMessage GetMessage(string uid, bool headersonly = false) {
			return GetMessage(uid, headersonly, true);
		}

		public virtual MailMessage GetMessage(int index, bool headersonly = false) {
			return GetMessage(index, headersonly, true);
		}

		public virtual MailMessage GetMessage(int index, bool headersonly, bool setseen) {
			return GetMessages(index, index, headersonly, setseen).FirstOrDefault();
		}

		public virtual MailMessage GetMessage(string uid, bool headersonly, bool setseen) {
			return GetMessages(uid, uid, headersonly, setseen).FirstOrDefault();
		}

		public virtual MailMessage[] GetMessages(string startUID, string endUID, bool headersonly = true, bool setseen = false) {
			return GetMessages(startUID, endUID, true, headersonly, setseen);
		}

		public virtual MailMessage[] GetMessages(int startIndex, int endIndex, bool headersonly = true, bool setseen = false) {
			return GetMessages((startIndex + 1).ToString(), (endIndex + 1).ToString(), false, headersonly, setseen);
		}

		public virtual void DownloadMessage(System.IO.Stream stream, int index, bool setseen) {
			GetMessages((index + 1).ToString(), (index + 1).ToString(), false, false, setseen, (message, size, headers) => {
				Utilities.CopyStream(message, stream, size);
				return null;
			});
		}

		public virtual void DownloadMessage(System.IO.Stream stream, string uid, bool setseen) {
			GetMessages(uid, uid, true, false, setseen, (message, size, headers) => {
				Utilities.CopyStream(message, stream, size);
				return null;
			});
		}

		public virtual MailMessage[] GetMessages(string start, string end, bool uid, bool headersonly, bool setseen) {
			var x = new List<MailMessage>();

			GetMessages(start, end, uid, headersonly, setseen, (stream, size, imapHeaders) => {
				var mail = new MailMessage { Encoding = Encoding };
				mail.Size = size;

				if (imapHeaders["UID"] != null)
					mail.Uid = imapHeaders["UID"];

				if (imapHeaders["Flags"] != null)
					mail.SetFlags(imapHeaders["Flags"]);

				mail.Load(_Stream, headersonly, mail.Size);

				foreach (var key in imapHeaders.AllKeys.Except(new[] { "UID", "Flags", "BODY[]", "BODY[HEADER]" }, StringComparer.OrdinalIgnoreCase))
					mail.Headers.Add(key, new HeaderValue(imapHeaders[key]));

				x.Add(mail);

				return mail;
			});

			return x.ToArray();
		}

		public virtual void GetMessages(string start, string end, bool uid, bool headersonly, bool setseen, Func<System.IO.Stream, int, NameValueCollection, MailMessage> action) {
			CheckMailboxSelected();
			IdlePause();

			string tag = GetTag();
			string command = tag + (uid ? "UID " : null)
				+ "FETCH " + start + ":" + end + " ("
				+ _FetchHeaders + "UID FLAGS BODY"
				+ (setseen ? null : ".PEEK")
				+ "[" + (headersonly ? "HEADER" : null) + "])";

			string response;

			SendCommand(command);
			while (true) {
				response = GetResponse();
				if (string.IsNullOrEmpty(response) || response.Contains(tag + "OK"))
					break;

				if (response[0] != '*' || !response.Contains("FETCH ("))
					continue;

				var imapHeaders = Utilities.ParseImapHeader(response.Substring(response.IndexOf('(') + 1));
				var size = (imapHeaders["BODY[HEADER]"] ?? imapHeaders["BODY[]"]).Trim('{', '}').ToInt();
				var msg = action(_Stream, size, imapHeaders);

				response = GetResponse();
				var n = response.Trim().LastOrDefault();
				if (n != ')') {
					System.Diagnostics.Debugger.Break();
					RaiseWarning(null, "Expected \")\" in stream, but received \"" + response + "\"");
				}
			}

			IdleResume();
		}

		public virtual Quota GetQuota(string mailbox) {
			if (!Supports("NAMESPACE"))
				new Exception("This command is not supported by the server!");
			IdlePause();

			Quota quota = null;
			string command = GetTag() + "GETQUOTAROOT " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			string response = SendCommandGetResponse(command);
			string reg = "\\* QUOTA (.*?) \\((.*?) (.*?) (.*?)\\)";
			while (response.StartsWith("*")) {
				Match m = Regex.Match(response, reg);
				if (m.Groups.Count > 1) {
					quota = new Quota(m.Groups[1].ToString(),
															m.Groups[2].ToString(),
															Int32.Parse(m.Groups[3].ToString()),
															Int32.Parse(m.Groups[4].ToString())
													);
					break;
				}
				response = GetResponse();
			}

			IdleResume();
			return quota;
		}

		public virtual Mailbox[] ListMailboxes(string reference, string pattern) {
			IdlePause();

			var x = new List<Mailbox>();
			string command = GetTag() + "LIST " + reference.QuoteString() + " " + pattern.QuoteString();
			string reg = "\\* LIST \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"?([^\\\"]+)\\\"?";
			string response = SendCommandGetResponse(command);
			Match m = Regex.Match(response, reg);
			while (m.Groups.Count > 1) {
				Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
				x.Add(mailbox);
				response = GetResponse();
				m = Regex.Match(response, reg);
			}
			IdleResume();
			return x.ToArray();
		}

		public virtual Mailbox[] ListSuscribesMailboxes(string reference, string pattern) {
			IdlePause();

			var x = new List<Mailbox>();
			string command = GetTag() + "LSUB " + reference.QuoteString() + " " + pattern.QuoteString();
			string reg = "\\* LSUB \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"([^\\\"]+)\\\"";
			string response = SendCommandGetResponse(command);
			Match m = Regex.Match(response, reg);
			while (m.Groups.Count > 1) {
				Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
				x.Add(mailbox);
				response = GetResponse();
				m = Regex.Match(response, reg);
			}
			IdleResume();
			return x.ToArray();
		}

		internal override void OnLogin(string login, string password) {
			string command = String.Empty;
			string result = String.Empty;
			string tag = GetTag();
			string key;

			switch (AuthMethod) {
				case AuthMethods.CRAMMD5:
					command = tag + "AUTHENTICATE CRAM-MD5";
					result = SendCommandGetResponse(command);
					// retrieve server key
					key = result.Replace("+ ", "");
					key = System.Text.Encoding.Default.GetString(Convert.FromBase64String(key));
					// calcul hash
					using (var kMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(password))) {
						byte[] hash1 = kMd5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(key));
						key = BitConverter.ToString(hash1).ToLower().Replace("-", "");
						result = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(login + " " + key));
						result = SendCommandGetResponse(result);
					}
					break;

				case AuthMethods.Login:
					command = tag + "LOGIN " + login.QuoteString() + " " + password.QuoteString();
					result = SendCommandGetResponse(command);
					break;

				case AuthMethods.SaslOAuth:
					command = tag + "AUTHENTICATE XOAUTH " + password;
					result = SendCommandGetResponse(command);
					break;

				default:
					throw new NotSupportedException();
			}

			if (result.StartsWith("* CAPABILITY ")) {
				_Capability = result.Substring(13).Trim().Split(' ');
				result = GetResponse();
			}

			if (!result.StartsWith(tag + "OK")) {
				throw new Exception(result);
			}

			//if (Supports("COMPRESS=DEFLATE")) {
			//  SendCommandCheckOK(GetTag() + "compress deflate");
			//  _Stream0 = _Stream;
			// // _Reader = new System.IO.StreamReader(new System.IO.Compression.DeflateStream(_Stream0, System.IO.Compression.CompressionMode.Decompress, true), System.Text.Encoding.Default);
			// // _Stream = new System.IO.Compression.DeflateStream(_Stream0, System.IO.Compression.CompressionMode.Compress, true);
			//}

			if (Supports("X-GM-EXT-1")) {
				_FetchHeaders = "X-GM-MSGID X-GM-THRID X-GM-LABELS ";
			}
		}

		internal override void OnLogout() {
			if (IsConnected)
				SendCommand(GetTag() + "LOGOUT");
		}

		public virtual Namespaces Namespace() {
			if (!Supports("NAMESPACE"))
				throw new NotSupportedException("This command is not supported by the server!");
			IdlePause();

			string command = GetTag() + "NAMESPACE";
			string response = SendCommandGetResponse(command);

			if (!response.StartsWith("* NAMESPACE")) {
				throw new Exception("Unknow server response !");
			}

			response = response.Substring(12);
			Namespaces n = new Namespaces();
			//[TODO] be sure to parse correctly namespace when not all namespaces are present. NIL character
			string reg = @"\((.*?)\) \((.*?)\) \((.*?)\)$";
			Match m = Regex.Match(response, reg);
			if (m.Groups.Count != 4)
				throw new Exception("En error occure, this command is not fully supported !");
			string reg2 = "\\(\\\"(.*?)\\\" \\\"(.*?)\\\"\\)";
			Match m2 = Regex.Match(m.Groups[1].ToString(), reg2);
			while (m2.Groups.Count > 1) {
				n.ServerNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
				m2 = m2.NextMatch();
			}
			m2 = Regex.Match(m.Groups[2].ToString(), reg2);
			while (m2.Groups.Count > 1) {
				n.UserNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
				m2 = m2.NextMatch();
			}
			m2 = Regex.Match(m.Groups[3].ToString(), reg2);
			while (m2.Groups.Count > 1) {
				n.SharedNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
				m2 = m2.NextMatch();
			}
			GetResponse();
			IdleResume();
			return n;
		}

		public virtual int GetMessageCount() {
			CheckMailboxSelected();
			return GetMessageCount(null);
		}
		public virtual int GetMessageCount(string mailbox) {
			IdlePause();

			string command = GetTag() + "STATUS " + Utilities.QuoteString(ModifiedUtf7Encoding.Encode(mailbox) ?? _SelectedMailbox) + " (MESSAGES)";
			string response = SendCommandGetResponse(command);
			string reg = @"\* STATUS.*MESSAGES (\d+)";
			int result = 0;
			while (response.StartsWith("*")) {
				Match m = Regex.Match(response, reg);
				if (m.Groups.Count > 1)
					result = Convert.ToInt32(m.Groups[1].ToString());
				response = GetResponse();
				m = Regex.Match(response, reg);
			}
			IdleResume();
			return result;
		}

		public virtual void RenameMailbox(string frommailbox, string tomailbox) {
			IdlePause();

			string command = GetTag() + "RENAME " + frommailbox.QuoteString() + " " + tomailbox.QuoteString();
			SendCommandCheckOK(command);
			IdleResume();
		}

		public virtual string[] Search(SearchCondition criteria, bool uid = true) {
			return Search(criteria.ToString(), uid);
		}

		public virtual string[] Search(string criteria, bool uid = true) {
			CheckMailboxSelected();

			string isuid = uid ? "UID " : "";
			string tag = GetTag();
			string command = tag + isuid + "SEARCH " + criteria;
			string response = SendCommandGetResponse(command);

			if (!response.StartsWith("* SEARCH", StringComparison.InvariantCultureIgnoreCase) && !IsResultOK(response)) {
				throw new Exception(response);
			}

			string temp;
			while (!(temp = GetResponse()).StartsWith(tag)) {
				response += Environment.NewLine + temp;
			}

			var m = Regex.Match(response, @"^\* SEARCH (.*)");
			return m.Groups[1].Value.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
		}

		public virtual Lazy<MailMessage>[] SearchMessages(SearchCondition criteria, bool headersonly = false, bool setseen = false) {
			return Search(criteria, true)
					.Select(x => new Lazy<MailMessage>(() => GetMessage(x, headersonly, setseen)))
					.ToArray();
		}

		public virtual Mailbox SelectMailbox(string mailbox) {
			IdlePause();

			mailbox = ModifiedUtf7Encoding.Encode(mailbox);
			Mailbox x = null;
			string tag = GetTag();
			string command = tag + "SELECT " + mailbox.QuoteString();
			string response = SendCommandGetResponse(command);
			if (response.StartsWith("*")) {
				x = new Mailbox(mailbox);
				while (response.StartsWith("*")) {
					Match m;
					m = Regex.Match(response, @"(\d+) EXISTS");
					if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
					m = Regex.Match(response, @"(\d+) RECENT");
					if (m.Groups.Count > 1)
						x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
					m = Regex.Match(response, @"UNSEEN (\d+)");
					if (m.Groups.Count > 1)
						x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
					m = Regex.Match(response, @" FLAGS \((.*?)\)");
					if (m.Groups.Count > 1)
						x.SetFlags(m.Groups[1].ToString());
					response = GetResponse();
				}
				if (IsResultOK(response)) {
					x.IsWritable = Regex.IsMatch(response, "READ.WRITE", RegexOptions.IgnoreCase);
				}
				_SelectedMailbox = mailbox;
			} else {
				throw new Exception(response);
			}
			IdleResume();
			return x;
		}

		public virtual void SetFlags(Flags flags, params MailMessage[] msgs) {
			SetFlags(FlagsToFlagString(flags), msgs);
		}

		public virtual void SetFlags(string flags, params MailMessage[] msgs) {
			Store("UID " + string.Join(" ", msgs.Select(x => x.Uid)), true, flags);
			foreach (var msg in msgs) {
				msg.SetFlags(flags);
			}
		}

		private string FlagsToFlagString(Flags flags) {
			return string.Join(" ", flags.ToString().Split(',').Select(x => "\\" + x.Trim()));
		}


		public virtual void AddFlags(Flags flags, params MailMessage[] msgs) {
			AddFlags(FlagsToFlagString(flags), msgs);
		}

		public virtual void AddFlags(string flags, params MailMessage[] msgs) {
			Store("UID " + string.Join(" ", msgs.Select(x => x.Uid)), false, flags);
			foreach (var msg in msgs) {
				msg.SetFlags(FlagsToFlagString(msg.Flags) + " " + flags);
			}
		}

		public virtual void Store(string messageset, bool replace, string flags) {
			CheckMailboxSelected();
			IdlePause();
			string prefix = null;
			if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
				messageset = messageset.Substring(4);
				prefix = "UID ";
			}

			string command = string.Concat(GetTag(), prefix, "STORE ", messageset, " ", replace ? "" : "+", "FLAGS.SILENT (" + flags + ")");
			string response = SendCommandGetResponse(command);
			while (response.StartsWith("*")) {
				response = GetResponse();
			}
			CheckResultOK(response);
			IdleResume();
		}

		public virtual void SuscribeMailbox(string mailbox) {
			IdlePause();

			string command = GetTag() + "SUBSCRIBE " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			SendCommandCheckOK(command);
			IdleResume();
		}

		public virtual void UnSuscribeMailbox(string mailbox) {
			IdlePause();

			string command = GetTag() + "UNSUBSCRIBE " + ModifiedUtf7Encoding.Encode(mailbox).QuoteString();
			SendCommandCheckOK(command);
			IdleResume();
		}

		internal override void CheckResultOK(string response) {
			if (!IsResultOK(response)) {
				response = response.Substring(response.IndexOf(" ")).Trim();
				throw new Exception(response);
			}
		}

		internal bool IsResultOK(string response) {
			response = response.Substring(response.IndexOf(" ")).Trim();
			return response.ToUpper().StartsWith("OK");
		}
	}
}

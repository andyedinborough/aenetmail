using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
	public class Pop3Client : TextClient, IMailClient {
		public Pop3Client() { }

		public Pop3Client(string host, string username, string password) {
			int port = 110;
			bool secure = false;
			bool skipSslValidation = false;

			Connect(host, port, secure, skipSslValidation);
			Login(username, password);
		}

		public Pop3Client(string host, string username, string password, int port, bool secure, bool skipSslValidation) {
			Connect(host, port, secure, skipSslValidation);
			Login(username, password);
		}

		internal override void OnLogin(string username, string password) {
			SendCommandCheckOK("USER " + username);
			SendCommandCheckOK("PASS " + password);
		}

		internal override void OnLogout() {
			if (_Stream != null) {
				SendCommand("QUIT");
			}
		}

		internal override void CheckResultOK(string result) {
			if (!result.StartsWith("+OK", StringComparison.OrdinalIgnoreCase)) {
				throw new Exception(result.Substring(result.IndexOf(' ') + 1).Trim());
			}
		}

		public virtual int GetMessageCount() {
			CheckConnectionStatus();
			var result = SendCommandGetResponse("STAT");
			CheckResultOK(result);
			return int.Parse(result.Split(' ')[1]);
		}

		//public virtual string[] GetMessageIDs() {
		//    CheckConnectionStatus();
		//    string result = SendCommandGetResponse("LIST");
		//    List<string> ids = new List<string>();
		//    while (result != ".") {
		//        result = _Reader.ReadLine();
		//        int i = result.IndexOf(' ');
		//        if (i > -1)
		//            ids.Add(result.Substring(0, i + 1));
		//    }

		//    return ids.ToArray();
		//}

		public virtual MailMessage GetMessage(int index) {
			bool headersOnly = false;
			return GetMessage((index + 1).ToString(), headersOnly);
		}

		public virtual MailMessage GetMessage(int index, bool headersOnly) {
			return GetMessage((index + 1).ToString(), headersOnly);
		}
		public MailMessage GetMessage(string uid) {
			bool headersOnly = false;
			return GetMessage(uid, headersOnly);
		}

		private static Regex rxOctets = new Regex(@"(\d+)\s+octets", RegexOptions.IgnoreCase);
		public MailMessage GetMessage(string uid, bool headersOnly) {
			CheckConnectionStatus();
			var result = new StringBuilder();
			string line = SendCommandGetResponse(string.Format(headersOnly ? "TOP {0} 0" : "RETR {0}", uid));
			var size = rxOctets.Match(line).Groups[1].Value.ToInt();
			CheckResultOK(line);
			var msg = new MailMessage();
			msg.Load(_Stream, headersOnly, size, '.');

			msg.Uid = uid;
			var last = GetResponse();
			if (string.IsNullOrEmpty(last))
				last = GetResponse();
			System.Diagnostics.Debug.Assert(last == ".");
			return msg;
		}

		public virtual void DeleteMessage(string uid) {
			SendCommandCheckOK("DELE " + uid);

		}

		public virtual void DeleteMessage(int index) {
			DeleteMessage((index + 1).ToString());
		}

		public virtual void DeleteMessage(AE.Net.Mail.MailMessage msg) {
			DeleteMessage(msg.Uid);
		}
	}
}
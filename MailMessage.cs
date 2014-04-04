using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AE.Net.Mail {
	public enum MailPriority {
		Normal = 3,
		High = 5,
		Low = 1
	}

	[System.Flags]
	public enum Flags {
		None = 0,
		Seen = 1,
		Answered = 2,
		Flagged = 4,
		Deleted = 8,
		Draft = 16
	}

	public class MailMessage : ObjectWHeaders {
		public static implicit operator System.Net.Mail.MailMessage(MailMessage msg) {
			var ret = new System.Net.Mail.MailMessage();
			ret.Subject = msg.Subject;
			ret.Sender = msg.Sender;
			foreach (var a in msg.Bcc)
				ret.Bcc.Add(a);
			ret.Body = msg.Body;
			ret.IsBodyHtml = msg.ContentType.Contains("html");
			ret.From = msg.From;
			ret.Priority = (System.Net.Mail.MailPriority)msg.Importance;
			foreach (var a in msg.ReplyTo)
				ret.ReplyToList.Add(a);
			foreach (var a in msg.To)
				ret.To.Add(a);
			foreach (var a in msg.Attachments)
				ret.Attachments.Add(new System.Net.Mail.Attachment(new System.IO.MemoryStream(a.GetData()), a.Filename, a.ContentType));
			foreach (var a in msg.AlternateViews)
				ret.AlternateViews.Add(new System.Net.Mail.AlternateView(new System.IO.MemoryStream(a.GetData()), a.ContentType));

			return ret;
		}

		private bool _HeadersOnly; // set to true if only headers have been fetched. 

		public MailMessage() {
			RawFlags = new string[0];
			To = new Collection<MailAddress>();
			Cc = new Collection<MailAddress>();
			Bcc = new Collection<MailAddress>();
			ReplyTo = new Collection<MailAddress>();
			Attachments = new Collection<Attachment>();
			AlternateViews = new AlternateViewCollection();
		}

		public virtual DateTime Date { get; set; }
		public virtual string[] RawFlags { get; set; }
		public virtual Flags Flags { get; set; }

		public virtual int Size { get; internal set; }
		public virtual string Subject { get; set; }
		public virtual ICollection<MailAddress> To { get; private set; }
		public virtual ICollection<MailAddress> Cc { get; private set; }
		public virtual ICollection<MailAddress> Bcc { get; private set; }
		public virtual ICollection<MailAddress> ReplyTo { get; private set; }
		public virtual ICollection<Attachment> Attachments { get; set; }
		public virtual AlternateViewCollection AlternateViews { get; set; }
		public virtual MailAddress From { get; set; }
		public virtual MailAddress Sender { get; set; }
		public virtual string MessageID { get; set; }
		public virtual string Uid { get; internal set; }
		public virtual MailPriority Importance { get; set; }

		public virtual void Load(string message, bool headersOnly = false) {
			if (string.IsNullOrEmpty(message)) return;
			using (var mem = new MemoryStream(_DefaultEncoding.GetBytes(message))) {
				Load(mem, headersOnly, message.Length);
			}
		}

		public virtual void Load(Stream reader, bool headersOnly = false, int maxLength = 0, char? termChar = null) {
			_HeadersOnly = headersOnly;
			Headers = null;
			Body = null;


			var headers = new StringBuilder();
			string line;
			while ((line = reader.ReadLine(ref maxLength, _DefaultEncoding, termChar)) != null) {
				if (line.Trim().Length == 0)
					if (headers.Length == 0)
						continue;
					else
						break;
				headers.AppendLine(line);
			}
			RawHeaders = headers.ToString();

			if (!headersOnly) {
				string boundary = Headers.GetBoundary();
				if (!string.IsNullOrEmpty(boundary)) {
					var atts = new List<Attachment>();
					var body = ParseMime(reader, boundary, ref maxLength, atts, Encoding, termChar);
					if (!string.IsNullOrEmpty(body))
						SetBody(body);

					foreach (var att in atts)
						(att.IsAttachment ? Attachments : AlternateViews).Add(att);

					if (maxLength > 0)
						reader.ReadToEnd(maxLength, Encoding);
				} else {
					//	sometimes when email doesn't have a body, we get here with maxLength == 0 and we shouldn't read any further
					string body = String.Empty;
					if (maxLength > 0)
						body = reader.ReadToEnd(maxLength, Encoding);

					SetBody(body);
				}
			}

			if ((string.IsNullOrWhiteSpace(Body) || ContentType.StartsWith("multipart/")) && AlternateViews.Count > 0) {
				var att = AlternateViews.GetTextView() ?? AlternateViews.GetHtmlView();
				if (att != null) {
					Body = att.Body;
					ContentTransferEncoding = att.Headers["Content-Transfer-Encoding"].RawValue;
					ContentType = att.Headers["Content-Type"].RawValue;
				}
			}

			Date = Headers.GetDate();
			To = Headers.GetMailAddresses("To").ToList();
			Cc = Headers.GetMailAddresses("Cc").ToList();
			Bcc = Headers.GetMailAddresses("Bcc").ToList();
			Sender = Headers.GetMailAddresses("Sender").FirstOrDefault();
			ReplyTo = Headers.GetMailAddresses("Reply-To").ToList();
			From = Headers.GetMailAddresses("From").FirstOrDefault();
			MessageID = Headers["Message-ID"].RawValue;

			Importance = Headers.GetEnum<MailPriority>("Importance");
			Subject = Headers["Subject"].RawValue;
		}

		private static string ParseMime(Stream reader, string boundary, ref int maxLength, ICollection<Attachment> attachments, Encoding encoding, char? termChar) {
			var maxLengthSpecified = maxLength > 0;
			string data = null,
					bounderInner = "--" + boundary,
					bounderOuter = bounderInner + "--";
			var n = 0;
			var body = new System.Text.StringBuilder();
			do {
				if (maxLengthSpecified && maxLength <= 0)
					return body.ToString();
				if (data != null) {
					body.Append(data);
				}
				data = reader.ReadLine(ref maxLength, encoding, termChar);
				n++;
			} while (data != null && !data.StartsWith(bounderInner));

			while (data != null && !data.StartsWith(bounderOuter) && !(maxLengthSpecified && maxLength == 0)) {
				data = reader.ReadLine(ref maxLength, encoding, termChar);
				if (data == null) break;
				var a = new Attachment { Encoding = encoding };

				var part = new StringBuilder();
				// read part header
				while (!data.StartsWith(bounderInner) && data != string.Empty && !(maxLengthSpecified && maxLength == 0)) {
					part.AppendLine(data);
					data = reader.ReadLine(ref maxLength, encoding, termChar);
					if (data == null) break;
				}
				a.RawHeaders = part.ToString();
				// header body

				// check for nested part
				var nestedboundary = a.Headers.GetBoundary();
				if (!string.IsNullOrEmpty(nestedboundary)) {
					ParseMime(reader, nestedboundary, ref maxLength, attachments, encoding, termChar);
					while (!data.StartsWith(bounderInner) && !(maxLengthSpecified && maxLength == 0))
						data = reader.ReadLine(ref maxLength, encoding, termChar);
				} else {
					data = reader.ReadLine(ref maxLength, a.Encoding, termChar);
					if (data == null) break;
					var nestedBody = new StringBuilder();
					while (!data.StartsWith(bounderInner) && !(maxLengthSpecified && maxLength == 0)) {
						nestedBody.AppendLine(data);
						data = reader.ReadLine(ref maxLength, a.Encoding, termChar);
					}
					a.SetBody(nestedBody.ToString());
					attachments.Add(a);
				}
			}
			return body.ToString();
		}

		private static Dictionary<string, int> _FlagCache = System.Enum.GetValues(typeof(Flags)).Cast<Flags>().ToDictionary(x => x.ToString(), x => (int)x, StringComparer.OrdinalIgnoreCase);
		internal void SetFlags(string flags) {
			RawFlags = flags.Split(' ').Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			Flags = (Flags)RawFlags.Select(x => {
				int flag = 0;
				if (_FlagCache.TryGetValue(x.TrimStart('\\'), out flag))
					return flag;
				else
					return 0;
			}).Sum();
		}

		public virtual void Save(System.IO.Stream stream, Encoding encoding = null) {
			using (var str = new System.IO.StreamWriter(stream, encoding ?? System.Text.Encoding.Default))
				Save(str);
		}

		private static readonly string[] SpecialHeaders = "Date,To,Cc,Reply-To,Bcc,Sender,From,Message-ID,Importance,Subject".Split(',');
		public virtual void Save(System.IO.TextWriter txt) {
			txt.WriteLine("Date: {0}", (Date == DateTime.MinValue ? DateTime.Now : Date).GetRFC2060Date());
			txt.WriteLine("To: {0}", string.Join("; ", To.Select(x => x.ToString())));
			txt.WriteLine("Cc: {0}", string.Join("; ", Cc.Select(x => x.ToString())));
			txt.WriteLine("Reply-To: {0}", string.Join("; ", ReplyTo.Select(x => x.ToString())));
			txt.WriteLine("Bcc: {0}", string.Join("; ", Bcc.Select(x => x.ToString())));
			if (Sender != null)
				txt.WriteLine("Sender: {0}", Sender);
			if (From != null)
				txt.WriteLine("From: {0}", From);
			if (!string.IsNullOrEmpty(MessageID))
				txt.WriteLine("Message-ID: {0}", MessageID);

			var otherHeaders = Headers.Where(x => !SpecialHeaders.Contains(x.Key, StringComparer.InvariantCultureIgnoreCase));
			foreach (var header in otherHeaders) {
				txt.WriteLine("{0}: {1}", header.Key, header.Value);
			}
			if (Importance != MailPriority.Normal)
				txt.WriteLine("Importance: {0}", (int)Importance);
			txt.WriteLine("Subject: {0}", Subject);

			string boundary = null;
			if (Attachments.Any() || AlternateViews.Any()) {
				boundary = string.Format("--boundary_{0}", Guid.NewGuid());
				txt.WriteLine("Content-Type: multipart/mixed; boundary={0}", boundary);
			}

			// signal end of headers
			txt.WriteLine();

			if (boundary != null) {
				txt.WriteLine("--" + boundary);
				txt.WriteLine();
			}

			txt.WriteLine(Body);

			AlternateViews.Union(Attachments).ToList().ForEach(att => {
				txt.WriteLine("--" + boundary);
				txt.WriteLine(string.Join("\n", att.Headers.Select(h => string.Format("{0}: {1}", h.Key, h.Value))));
				txt.WriteLine();
				txt.WriteLine(att.Body);
			});

			if (boundary != null) {
				txt.WriteLine("--" + boundary + "--");
			}
		}
	}
}

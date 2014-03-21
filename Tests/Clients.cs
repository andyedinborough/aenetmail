using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;

namespace Tests {
	[TestClass]
	public class UnitTest1 {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void IDLE() {
			var mre1 = new System.Threading.ManualResetEventSlim(false);
			var mre2 = new System.Threading.ManualResetEventSlim(false);
			using (var imap = GetClient<ImapClient>()) {
				bool fired = false;
				imap.MessageDeleted += (sender, e) => {
					fired = true;
					mre2.Set();
				};

				var count = imap.GetMessageCount();
				count.ShouldBeInRange(1, int.MaxValue); //interupt the idle thread

				System.Threading.ThreadPool.QueueUserWorkItem(_ => {
					Delete_Message();
					mre1.Set();
				});

				mre1.Wait();
				mre2.Wait(TimeSpan.FromSeconds(15));//give the other thread a moment
				fired.ShouldBe();
			}
		}

		[TestMethod]
		public void Message_With_Attachments() {
			using (var imap = GetClient<ImapClient>()) {
				var msg = imap.SearchMessages(SearchCondition.Larger(100 * 1000)).FirstOrDefault().Value;

				msg.Attachments.Count.ShouldBeInRange(1, int.MaxValue);

			}
		}

		[TestMethod]
		public void Select_Folder() {
			using (var imap = GetClient<ImapClient>()) {
				imap.SelectMailbox("Notes");
				imap.Examine("Notes").UIDValidity.ShouldBeGreaterThan(0);
				imap.GetMessageCount().ShouldBeInRange(1, int.MaxValue);
			}
		}

		[TestMethod]
		public void Polish_Characters() {
			using (var imap = GetClient<ImapClient>()) {
				var msg = imap.SearchMessages(SearchCondition.Subject("POLISH LANGUAGE TEST")).FirstOrDefault();
				msg.Value.ShouldBe();

				msg.Value.Body.ShouldContain("Cię e-mailem, kiedy Kupują");

			}
		}

		[TestMethod]
		public void POP() {
			using (var client = GetClient<Pop3Client>("gmail", "pop3")) {
				var msg = client.GetMessage(0);
				Console.WriteLine(msg.Body);
			}
		}

		[TestMethod]
		public void Connections() {
			var accountsToTest = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "\\aenetmail\\" }, StringSplitOptions.RemoveEmptyEntries).First(), "ae.net.mail.usernames.txt");
			var lines = System.IO.File.ReadAllLines(accountsToTest)
					.Select(x => x.Split(','))
					.Where(x => x.Length == 6)
					.ToArray();

			lines.Any(x => x[0] == "imap").ShouldBe();
			lines.Any(x => x[0] == "pop3").ShouldBe();

			foreach (var line in lines)
				using (var mail = GetClient(line[0], line[1], int.Parse(line[2]), bool.Parse(line[3]), line[4], line[5])) {
					mail.GetMessageCount().ShouldBeInRange(1, int.MaxValue);

					var msg = mail.GetMessage(0, true);
					msg.Subject.ShouldNotBeNullOrEmpty();
					msg = mail.GetMessage(0, false);
					msg.Body.ShouldNotBeNullOrEmpty();

					mail.Disconnect();
					mail.Disconnect();
				}
		}

		[TestMethod]
		public void Search_Conditions() {
			var deleted = SearchCondition.Deleted();
			var seen = SearchCondition.Seen();
			var text = SearchCondition.Text("andy");

			deleted.ToString().ShouldBe("DELETED");
			deleted.Or(seen).ToString().ShouldBe("OR (DELETED) (SEEN)");
			seen.And(text).ToString().ShouldBe("(SEEN) (TEXT \"andy\")");

			var since = new DateTime(2000, 1, 1);
			SearchCondition.Undeleted().And(
									SearchCondition.From("david"),
									SearchCondition.SentSince(since)
							).Or(SearchCondition.To("andy"))
					.ToString()
					.ShouldBe("OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"" + Utilities.GetRFC2060Date(since) + "\")) (TO \"andy\")");
		}

		[TestMethod]
		public void Search() {
			using (var imap = GetClient<ImapClient>()) {
				var result = imap.SearchMessages(
					//"OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"01-Jan-2000 00:00:00\")) (TO \"andy\")"
						SearchCondition.Undeleted().And(SearchCondition.From("david"), SearchCondition.SentSince(new DateTime(2000, 1, 1))).Or(SearchCondition.To("andy"))
						);
				result.Length.ShouldBeInRange(1, int.MaxValue);
				result.First().Value.Subject.ShouldNotBeNullOrEmpty();

				result = imap.SearchMessages(new SearchCondition { Field = SearchCondition.Fields.Text, Value = "asdflkjhdlki2uhiluha829hgas" });
				result.Length.ShouldBe(0);
			}
		}

		[TestMethod]
		public void Issue_49() {
			using (var client = GetClient<ImapClient>()) {
				var msg = client.SearchMessages(SearchCondition.Subject("aenetmail").And(SearchCondition.Subject("#49"))).Select(x => x.Value).FirstOrDefault();
				msg.ShouldBe();
				msg.AlternateViews.FirstOrDefault(x => x.ContentType.Contains("html")).Body.ShouldBe();
			}
		}

		[TestMethod]
		public void Append_Mail() {
			using (var client = GetClient<ImapClient>()) {
				var msg = new MailMessage {
					Subject = "TEST",
					Body = "Appended!"
				};
				msg.Date = DateTime.Now;

				client.AppendMail(msg, "Inbox");
			}
		}

		[TestMethod]
		public void Parse_Imap_Header() {
			var header = @"X-GM-THRID 1320777376118077475 X-GM-MSGID 1320777376118077475 X-GM-LABELS () UID 8286 RFC822.SIZE 9369 FLAGS (\Seen) BODY[] {9369}";

			var values = Utilities.ParseImapHeader(header);
			values["FLAGS"].ShouldBe(@"\Seen");
			values["UID"].ShouldBe("8286");
			values["X-GM-MSGID"].ShouldBe("1320777376118077475");
			values["X-GM-LABELS"].ShouldBeNullOrEmpty();
			values["RFC822.SIZE"].ShouldBe("9369");
		}

		[TestMethod]
		public void Get_Several_Messages() {
			int numMessages = 10;
			using (var imap = GetClient<ImapClient>()) {
				var msgs = imap.GetMessages(0, numMessages - 1, false);
				msgs.Length.ShouldBe(numMessages);

				for (var i = 0; i < 1000; i++) {
					var msg = imap.GetMessage(i);
					msg.Subject.ShouldNotBeNullOrEmpty();
					msg.Body.ShouldNotBeNullOrEmpty();
					msg.ContentType.ShouldStartWith("text/");
				}

				msgs = imap.GetMessages(0, numMessages - 1, true);
				msgs.Length.ShouldBe(numMessages);
				msgs.Count(x => string.IsNullOrEmpty(x.Subject)).ShouldBe(0);
			}
		}

		[TestMethod]
		public void Download_Message() {
			var filename = System.IO.Path.GetTempFileName();

			try {
				using (var imap = GetClient<ImapClient>())
				using (var file = new System.IO.FileStream(filename, System.IO.FileMode.Create)) {
					imap.DownloadMessage(file, 0, false);
				}

				using (var file = new System.IO.FileStream(filename, System.IO.FileMode.Open)) {
					var msg = new AE.Net.Mail.MailMessage();
					msg.Load(file);
					msg.Subject.ShouldNotBeNullOrEmpty();
				}

			} finally {
				System.IO.File.Delete(filename);
			}
		}

		[TestMethod]
		public void Delete_Message() {
			using (var client = GetClient<ImapClient>()) {
				var lazymsg = client.SearchMessages(SearchCondition.From("DRAGONEXT")).FirstOrDefault();
				var msg = lazymsg == null ? null : lazymsg.Value;
				msg.ShouldBe();

				var uid = msg.Uid;
				client.DeleteMessage(msg);

				msg = client.GetMessage(uid);
				Console.WriteLine(msg);
			}
		}

		private string GetSolutionDirectory() {
			var dir = new System.IO.DirectoryInfo(Environment.CurrentDirectory);
			while (dir.GetFiles("*.sln").Length == 0) {
				dir = dir.Parent;
			}
			return dir.FullName;
		}

		private T GetClient<T>(string host = "gmail", string type = "imap") where T : class, IMailClient {
			var accountsToTest = System.IO.Path.Combine(GetSolutionDirectory(), "..\\ae.net.mail.usernames.txt");
			var lines = System.IO.File.ReadAllLines(accountsToTest)
					.Select(x => x.Split(','))
					.Where(x => x.Length == 6)
					.ToArray();

			var line = lines.Where(x => x[0].Equals(type) && (x.ElementAtOrDefault(1) ?? string.Empty).Contains(host)).FirstOrDefault();
			return GetClient(line[0], line[1], int.Parse(line[2]), bool.Parse(line[3]), line[4], line[5]) as T;
		}

		private IMailClient GetClient(string type, string host, int port, bool ssl, string username, string password) {
			if ("imap".Equals(type, StringComparison.OrdinalIgnoreCase)) {
				return new AE.Net.Mail.ImapClient(host, username, password, AE.Net.Mail.AuthMethods.Login, port, ssl);
			}

			if ("pop3".Equals(type, StringComparison.OrdinalIgnoreCase)) {
				return new AE.Net.Mail.Pop3Client(host, username, password, port, ssl);
			}

			throw new NotImplementedException(type + " is not implemented");
		}
	}
}

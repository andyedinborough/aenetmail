using System;
using System.Linq;
using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Fluent;

namespace Tests {
  [TestClass]
  public class UnitTest1 {
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestIDLE() {
      var mre1 = new System.Threading.ManualResetEventSlim(false);
      var mre2 = new System.Threading.ManualResetEventSlim(false);
      using (var imap = GetClient<ImapClient>()) {
        bool fired = false;
        imap.MessageDeleted += (sender, e) => {
          fired = true;
          mre2.Set();
        };

        var count = imap.GetMessageCount();
        count.Should().Be.InRange(1, int.MaxValue); //interupt the idle thread

        System.Threading.ThreadPool.QueueUserWorkItem(_ => {
          TestDelete();
          mre1.Set();
        });

        mre1.Wait();
        mre2.Wait(TimeSpan.FromSeconds(15));//give the other thread a moment
        fired.Should().Be.True();
      }
    }

    [TestMethod]
    public void TestMessageWithAttachments() {
      using (var imap = GetClient<ImapClient>()) {
        var msg = imap.SearchMessages(SearchCondition.Larger(100 * 1000)).FirstOrDefault().Value;

        msg.Attachments.Count.Should().Be.InRange(1, int.MaxValue);

      }
    }

    [TestMethod]
    public void TestSelectFolder() {
      using (var imap = GetClient<ImapClient>()) {
        imap.SelectMailbox("Notes");
        imap.GetMessageCount().Should().Be.InRange(1, int.MaxValue);
      }
    }

    [TestMethod]
    public void TestPolish() {
      using (var imap = GetClient<ImapClient>()) {
        var msg = imap.SearchMessages(SearchCondition.Subject("POLISH LANGUAGE TEST")).FirstOrDefault();
        msg.Value.Should().Not.Be.Null();

        msg.Value.Body.Should().Contain("Cię e-mailem, kiedy Kupują");

      }
    }

    [TestMethod]
    public void TestConnections() {
      var accountsToTest = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "\\aenetmail\\" }, StringSplitOptions.RemoveEmptyEntries).First(), "ae.net.mail.usernames.txt");
      var lines = System.IO.File.ReadAllLines(accountsToTest)
          .Select(x => x.Split(','))
          .Where(x => x.Length == 6)
          .ToArray();

      lines.Any(x => x[0] == "imap").Should().Be.True();
      lines.Any(x => x[0] == "pop3").Should().Be.True();

      foreach (var line in lines)
        using (var mail = GetClient(line[0], line[1], int.Parse(line[2]), bool.Parse(line[3]), line[4], line[5])) {
          mail.GetMessageCount().Should().Be.InRange(1, int.MaxValue);

          var msg = mail.GetMessage(0, true);
          msg.Subject.Should().Not.Be.NullOrEmpty();
          msg = mail.GetMessage(0, false);
          msg.Body.Should().Not.Be.NullOrEmpty();

          mail.Disconnect();
          mail.Disconnect();
        }
    }

    [TestMethod]
    public void TestSearchConditions() {
      var deleted = SearchCondition.Deleted();
      var seen = SearchCondition.Seen();
      var text = SearchCondition.Text("andy");

      deleted.ToString().Should().Equal("DELETED");
      deleted.Or(seen).ToString().Should().Equal("OR (DELETED) (SEEN)");
      seen.And(text).ToString().Should().Equal("(SEEN) (TEXT \"andy\")");

      var since = new DateTime(2000, 1, 1);
      SearchCondition.Undeleted().And(
                  SearchCondition.From("david"),
                  SearchCondition.SentSince(since)
              ).Or(SearchCondition.To("andy"))
          .ToString()
          .Should().Equal("OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"" + Utilities.GetRFC2060Date(since) + "\")) (TO \"andy\")");
    }

    [TestMethod]
    public void TestSearch() {
      using (var imap = GetClient<ImapClient>()) {
        var result = imap.SearchMessages(
          //"OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"01-Jan-2000 00:00:00\")) (TO \"andy\")"
            SearchCondition.Undeleted().And(SearchCondition.From("david"), SearchCondition.SentSince(new DateTime(2000, 1, 1))).Or(SearchCondition.To("andy"))
            );
        result.Length.Should().Be.InRange(1, int.MaxValue);
        result.First().Value.Subject.Should().Not.Be.NullOrEmpty();

        result = imap.SearchMessages(new SearchCondition { Field = SearchCondition.Fields.Text, Value = "asdflkjhdlki2uhiluha829hgas" });
        result.Length.Should().Equal(0);
      }
    }

    [TestMethod]
    public void TestIssue49() {
      using (var client = GetClient<ImapClient>()) {
        var msg = client.SearchMessages(SearchCondition.Subject("aenetmail").And(SearchCondition.Subject("#49"))).Select(x => x.Value).FirstOrDefault();
        msg.Should().Not.Be.Null();
        msg.AlternateViews.FirstOrDefault(x=>x.ContentType.Contains("html")).Body.Should().Not.Be.Null();
      }
    }

    [TestMethod]
    public void TestAppendMail() {
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
    public void TestParseImapHeader() {
      var header = @"X-GM-THRID 1320777376118077475 X-GM-MSGID 1320777376118077475 X-GM-LABELS () UID 8286 RFC822.SIZE 9369 FLAGS (\Seen) BODY[] {9369}";

      var values = ImapClient.ParseImapHeader(header);
      values["FLAGS"].Should().Equal(@"\Seen");
      values["UID"].Should().Equal("8286");
      values["X-GM-MSGID"].Should().Equal("1320777376118077475");
      values["X-GM-LABELS"].Should().Be.NullOrEmpty();
      values["RFC822.SIZE"].Should().Equal("9369");
    }

    [TestMethod]
    public void TestGetSeveralMessages() {
      int numMessages = 1000;
      using (var imap = GetClient<ImapClient>()) {
        var msgs = imap.GetMessages(0, numMessages - 1, true);
        msgs.Length.Should().Equal(numMessages);
        msgs.Count(x => string.IsNullOrEmpty(x.Subject)).Should().Equal(0);
      }
      using (var imap = GetClient<ImapClient>()) {
        var msgs = imap.GetMessages(0, numMessages - 1, false);
        msgs.Length.Should().Equal(numMessages);
        msgs.Count(x => string.IsNullOrEmpty(x.Subject)).Should().Equal(0);
      }
    }

    [TestMethod]
    public void TestDelete() {
      using (var client = GetClient<ImapClient>()) {
        var lazymsg = client.SearchMessages(SearchCondition.From("DRAGONEXT")).FirstOrDefault();
        var msg = lazymsg == null ? null : lazymsg.Value;
        msg.Should().Not.Be.Null();

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
        return new AE.Net.Mail.ImapClient(host, username, password, AE.Net.Mail.ImapClient.AuthMethods.Login, port, ssl);
      }

      if ("pop3".Equals(type, StringComparison.OrdinalIgnoreCase)) {
        return new AE.Net.Mail.Pop3Client(host, username, password, port, ssl);
      }

      throw new NotImplementedException(type + " is not implemented");
    }
  }
}

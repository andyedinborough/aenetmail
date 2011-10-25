using System;
using System.Linq;
using AE.Net.Mail;
using AE.Net.Mail.Imap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Fluent;

namespace Tests {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1 {
        public TestContext TestContext { get; set; }

        #region
        string iphoneMessage = @"+OK 159354 octets
Received: from ([99.34.8.150]) for <edinboroughs@trac.ky> with MailEnable Catch-All Filter; Wed, 13 Jul 2011 09:25:50 -0700
Received: from main.edinborough.org ([99.34.8.150]) by mail.vortaloptics.com with MailEnable ESMTP; Wed, 13 Jul 2011 09:25:49 -0700
Received: from [192.168.1.140] ([192.168.1.140])
	by main.edinborough.org
	; Wed, 13 Jul 2011 11:29:40 -0500
Subject: =?utf-8?B?RnLDpW52YXJvOiDDlnJlYnJvIEdvbGZrbHViYiAtIFNjb3JlY2FyZA==?=
From: Andy Edinborough <andy@edinborough.org>
Content-Type: multipart/mixed; boundary=Apple-Mail-1--592579169
Message-Id: <AE28825C-F702-438E-8132-AD36D89792C8@edinborough.org>
Date: Wed, 13 Jul 2011 11:29:46 -0500
To: ""edinboroughs@trac.ky"" <edinboroughs@trac.ky>
Content-Transfer-Encoding: 7bit
Mime-Version: 1.0 (iPhone Mail 8J2)
X-Mailer: iPhone Mail (8J2)
Return-Path: <andy@edinborough.org>


--Apple-Mail-1--592579169
Content-Transfer-Encoding: 7bit
Content-Type: text/plain;
	charset=us-ascii

Due: tomorrow


--Apple-Mail-1--592579169
Content-Disposition: inline;
	filename=photo.JPG
Content-Type: image/jpeg;
	name=photo.JPG
Content-Transfer-Encoding: base64

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";
        #endregion

        [TestMethod]
        public void TestIDLE() {
            var mre = new System.Threading.ManualResetEvent(false);
            using (var imap = GetClient<ImapClient>()) {
                imap.SelectMailbox("inbox");
                imap.NewMessage += imap_NewMessage;

                while (!mre.WaitOne(5000)) //low for the sake of testing; typical timeout is 30 minutes
                    imap.Noop();
            }
        }

        void imap_NewMessage(object sender, MessageEventArgs e) {
            var imap = (sender as ImapClient);
            var msg = imap.GetMessage(e.MessageCount - 1);
            Console.WriteLine(msg.Subject);
        }

        [TestMethod]
        public void TestParseMessageFromIPhone() {
            var msg = GetMessage(iphoneMessage);

            msg.Attachments.Count.Should().Equal(2);
            msg.Attachments.All(a => a.GetContent().Any().Should().Be.True());
            msg.Subject.Should().Equal("Frånvaro: Örebro Golfklubb - Scorecard");
        }

        [TestMethod]
        public void TestBasicMessage() {
            var msg = GetMessage(@"From: test@localhost
To: root@localhost
Subject: DEAR FRIEND

THROUGH THE COURTESY OF BUSINESS OPPORTUNITY, I TAKE LIBERTY ANCHORED ON A
STRONG DESIRE TO SOLICIT YOUR ASSISTANCE ON THIS MUTUALLY BENEFICIAL AND
RISKFREE TRANSACTION WHICH I HOPE YOU WILL GIVE YOUR URGENT ATTENTION.

I HAVE DEPOSITED THE SUM OF THIRTY MILLION,FIVE HUNDRED THOUSAND UNITED
STATES DOLLARS(US$30,500,000) WITH A SECURITY COMPANY FOR SAFEKEEPING.
THE FUNDS ARE SECURITY CODED TO PREVENT THEM FROM KNOWING THE ACTUAL
CONTENTS.

MAY I AT THIS POINT EMPHASIZE THE HIGH LEVEL OF CONFIDENTIALLITY WHICH THIS
BUSINESS DEMANDS AND HOPE YOU WILL NOT BETRAY THE TRUST AND CONFIDENCE WHICH
WE REPOSE IN YOU.");

            msg.From.Should().Not.Be.Null();
            msg.To.Should().Not.Be.Null();
            msg.Subject.Should().Equal("DEAR FRIEND");
        }

        [TestMethod]
        public void TestBasicMimeMessage() {
            var msg = GetMessage(@"From: John Doe <example@example.com>
MIME-Version: 1.0
Content-Type: multipart/mixed;
        boundary=""XXXXboundary text""

This is a multipart message in MIME format.

--XXXXboundary text 
Content-Type: text/plain

this is the body text

--XXXXboundary text 
Content-Type: text/plain;
Content-Disposition: attachment;
        filename=""test.txt""

this is the attachment text

--XXXXboundary text--");

            msg.From.Should().Not.Be.Null();
            msg.Attachments.Count.Should().Equal(2);
            msg.Attachments.All(a => a.GetContent().Any().Should().Be.True());
        }

        private AE.Net.Mail.MailMessage GetMessage(string raw) {
            var msg = new AE.Net.Mail.MailMessage();
            msg.Load(raw, false);

            return msg;
        }

        [TestMethod]
        public void TestMethod1() {
            var accountsToTest = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "\\AE.Net.Mail\\" }, StringSplitOptions.RemoveEmptyEntries).First(), "ae.net.mail.usernames.txt");
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
                    (msg.Body + msg.BodyHtml).Should().Not.Be.NullOrEmpty();
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
                .Should().Equal("OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"" + SearchCondition.GetRFC2060Date(since) + "\")) (TO \"andy\")");
        }

        [TestMethod]
        public void TestSearch() {
            using (var imap = GetClient<ImapClient>()) {
                var result = imap.Search(
                    //"OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"01-Jan-2000 00:00:00\")) (TO \"andy\")"
                    SearchCondition.Undeleted().And(SearchCondition.From("david"), SearchCondition.SentSince(new DateTime(2000, 1, 1))).Or(SearchCondition.To("andy"))
                    );
                result.Length.Should().Be.InRange(1, int.MaxValue);

                result = imap.Search(new SearchCondition { Field = SearchCondition.Fields.Text, Value = "asdflkjhdlki2uhiluha829hgas" });
                result.Length.Should().Equal(0);
            }
        }

        private T GetClient<T>(string host = "gmail", string type = "imap") where T : class, IMailClient {
            var accountsToTest = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "\\AE.Net.Mail\\" }, StringSplitOptions.RemoveEmptyEntries).First(), "ae.net.mail.usernames.txt");
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

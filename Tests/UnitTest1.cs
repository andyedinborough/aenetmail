using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Fluent;

namespace Tests {
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1 {
        public TestContext TestContext { get; set; }

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

        private AE.Net.Mail.IMailClient GetClient(string type, string host, int port, bool ssl, string username, string password) {
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

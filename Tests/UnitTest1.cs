using System;
using System.Text;
using System.Collections.Generic;
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1() {
            var lines = System.IO.File.ReadAllLines(@"c:\repos\ae.net.mail.usernames.txt")
                .Select(x => x.Split(','))
                .Where(x => x.Length == 6)
                .ToArray();

            lines.Any(x => x[0] == "imap").Should().Be.True();
            lines.Any(x => x[0] == "pop3").Should().Be.True();

            foreach(var line in lines)
                using (var mail = GetClient(line[0], line[1], int.Parse(line[2]), bool.Parse(line[3]), line[4], line[5])) {
                    mail.GetMessageCount().Should().Be.InRange(1, int.MaxValue);
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

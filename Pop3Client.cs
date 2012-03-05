using System;
using System.Text;

namespace AE.Net.Mail {
    public class Pop3Client : TextClient, IMailClient {
        public Pop3Client() { }
        public Pop3Client(string host, string username, string password, int port = 110, bool secure = false, bool skipSslValidation = false)
        {
            Connect(host, port, secure, skipSslValidation);
            Login(username, password);
        }

        internal override void OnLogin(string username, string password) {
            SendCommandCheckOK("USER " + username);
            SendCommandCheckOK("PASS " + password);
        }

        internal override void OnLogout() {
            if (_Stream != null)
            {
                SendCommand("QUIT");
            }
        }

        internal override void CheckResultOK(string result) {
            if (!result.StartsWith("+OK", StringComparison.OrdinalIgnoreCase)) {
                throw new Exception(result.Substring(result.IndexOf(' ') + 1).Trim());
            }
        }

        public int GetMessageCount() {
            CheckConnectionStatus();
            var result = SendCommandGetResponse("STAT");
            CheckResultOK(result);
            return int.Parse(result.Split(' ')[1]);
        }

        //public string[] GetMessageIDs() {
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

        public MailMessage GetMessage(int index, bool headersOnly = false) {
            return GetMessage((index + 1).ToString(), headersOnly);
        }

        public MailMessage GetMessage(string uid, bool headersOnly = false) {
            CheckConnectionStatus();
            var result = new StringBuilder();
            string line = SendCommandGetResponse(string.Format(headersOnly ? "TOP {0} 0" : "RETR {0}", uid));
            while (line != ".") {
                result.AppendLine(line);
                line = GetResponse();
            }

            var msg = new MailMessage();
            msg.Load(result.ToString(), headersOnly);
            msg.Uid = uid;
            return msg;
        }

        public void DeleteMessage(string uid) {
            SendCommandCheckOK("DELE " + uid);

        }

        public void DeleteMessage(int index) {
            DeleteMessage((index + 1).ToString());
        }

        public void DeleteMessage(AE.Net.Mail.MailMessage msg) {
            DeleteMessage(msg.Uid);
        }
    }
}
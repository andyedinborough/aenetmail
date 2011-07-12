using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AE.Net.Mail {
    public class Pop3Client : TextClient, IMailClient {
        public Pop3Client() { }
        public Pop3Client(string host, string username, string password, int port = 110, bool secure = false) {
            Connect(host, port, secure);
            Login(username, password);
        }

        internal override void OnLogin(string username, string password) {
            SendCommandCheckOK(string.Format("USER {0}", username));
            SendCommandCheckOK(string.Format("PASS {0}", password));
        }

        internal override void OnLogout() {
            SendCommand("QUIT");
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

        public MailMessage GetMessage(int index, bool headersOnly) {
            return GetMessage((index + 1).ToString(), headersOnly);
        }

        public MailMessage GetMessage(string uid, bool headersOnly = false) {
            CheckConnectionStatus();
            var result = new StringBuilder();
            string line = SendCommandGetResponse(string.Format(headersOnly ? "TOP {0} 0" : "RETR {0}", uid));
            while (line != ".") {
                result.AppendLine(line);
                line = _Reader.ReadLine();
            }

            var msg = new MailMessage();
            msg.Load(result.ToString(), headersOnly);
            msg.Uid = uid;
            return msg;
        }

        public void DeleteMessage(string uid) {
            SendCommandCheckOK("DELE {0}" + uid);
          
        }

        public void DeleteMessage(int index) {
            DeleteMessage((index + 1).ToString());
        }
    }
}
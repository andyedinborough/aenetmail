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
            return GetMessageIDs().Length;
        }

        public string[] GetMessageIDs() {
            CheckConnectionStatus();
            string result = SendCommandGetResponse("LIST");
            List<string> ids = new List<string>();
            while (result != ".") {
                result = _Reader.ReadLine();
                int i = result.IndexOf(' ');
                if (i > -1)
                    ids.Add(result.Substring(i + 1));
            }

            return ids.ToArray();
        }

        public MailMessage GetMessage(int index, bool headersOnly) {
            CheckConnectionStatus();
            StringBuilder result = new StringBuilder();
            string line = SendCommandGetResponse(string.Format("RETR {0}{1}", index + 1, headersOnly ? " 0" : string.Empty));
            while (line != ".") {
                result.AppendLine(line);
                line = _Reader.ReadLine();
            }

            MailMessage msg = new MailMessage();
            msg.Load(result.ToString(), headersOnly);
            return msg;
        }

        public void DeleteMessage(string uid) {
            var ids = GetMessageIDs();
            for (int i = 0; i < ids.Length; i++) {
                if (ids[i] == uid) {
                    DeleteMessage(i);
                    return;
                }
            }
        }

        public void DeleteMessage(int index) {
            SendCommandCheckOK(string.Format("DELE {0}", index + 1));
        }
    }
}
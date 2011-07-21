using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AE.Net.Mail.Imap;

namespace AE.Net.Mail {
    public class ImapClient : TextClient, IMailClient {
        private string _selectedmailbox;
        private int _tag = 0;
        private string _capability;

        public ImapClient(string host, string username, string password, AuthMethods method = AuthMethods.Login, int port = 143, bool secure = false) {
            Connect(host, port, secure);
            AuthMethod = method;
            Login(username, password);
        }

        public enum AuthMethods {
            Login, CRAMMD5
        }

        public AuthMethods AuthMethod { get; set; }

        private string GetTag() {
            _tag++;
            return string.Format("xm{0:000} ", _tag);
        }

        public void AppendMail(string mailbox, MailMessage email) {
            CheckConnectionStatus();

            string flags = String.Empty;
            string size = (email.Body.Length - 1).ToString();
            if (email.Flags.Length > 0) {
                flags = string.Concat("(", string.Join(" ", email.Flags), ")");
            }
            string command = GetTag() + "APPEND " + mailbox + " " + flags + " {" + size + "}";
            string response = SendCommandGetResponse(command);
            if (response.StartsWith("+")) {
                response = SendCommandGetResponse(email.Body);
            }
        }

        public string Capability() {
            if (!IsConnected) throw new Exception("You must connect first !");
            string command = GetTag() + "CAPABILITY";
            string response = SendCommandGetResponse(command);
            if (response.StartsWith("* CAPABILITY ")) response = response.Substring(13);
            _capability = response;
            _Reader.ReadLine();
            return response;
        }

        public void Copy(string messageset, string destination) {
            CheckMailboxSelected();
            string prefix = null;
            if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
                messageset = messageset.Substring(4);
                prefix = "UID ";
            }
            string command = string.Concat(GetTag(), prefix, "COPY ", messageset, " \"" + destination + "\"");
            SendCommandCheckOK(command);
        }

        public void CreateMailbox(string mailbox) {
            CheckConnectionStatus();
            string command = GetTag() + "CREATE \"" + mailbox + "\"";
            SendCommandCheckOK(command);
        }

        public void DeleteMailbox(string mailbox) {
            CheckConnectionStatus();

            string command = GetTag() + "DELETE \"" + mailbox + "\"";
            SendCommandCheckOK(command);
        }

        public Mailbox Examine(string mailbox) {
            CheckConnectionStatus();


            Mailbox x = null;
            string tag = GetTag();
            string command = tag + "EXAMINE \"" + mailbox + "\"";
            string response = SendCommandGetResponse(command);
            if (response.StartsWith("*")) {
                x = new Mailbox(mailbox);
                while (response.StartsWith("*")) {
                    Match m;
                    m = Regex.Match(response, @"(\d+) EXISTS");
                    if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
                    m = Regex.Match(response, @"(\d+) RECENT");
                    if (m.Groups.Count > 1) x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
                    m = Regex.Match(response, @"UNSEEN (\d+)");
                    if (m.Groups.Count > 1) x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
                    m = Regex.Match(response, @" FLAGS \((.*?)\)");
                    if (m.Groups.Count > 1) x.SetFlags(m.Groups[1].ToString());
                    response = _Reader.ReadLine();
                }
                if (response.StartsWith(tag + "OK")) {
                    if (response.ToUpper().IndexOf("READ/WRITE") > -1) x.Rw = true;
                }
                _selectedmailbox = mailbox;
            }
            return x;
        }

        public void Expunge() {
            CheckMailboxSelected();

            string tag = GetTag();
            string command = tag + "EXPUNGE";
            string response = SendCommandGetResponse(command);
            while (response.StartsWith("*")) {
                response = _Reader.ReadLine();
            }
        }

        public void DeleteMessage(AE.Net.Mail.MailMessage msg) {
            DeleteMessage(msg.Uid);
        }

        public void DeleteMessage(string uid) {
            CheckMailboxSelected();
            Store("UID " + uid, true, "\\Seen \\Deleted");
        }

        public void MoveMessage(string uid, string folderName) {
            CheckMailboxSelected();
            Copy("UID " + uid, folderName);
            DeleteMessage(uid);
        }

        private void CheckMailboxSelected() {
            CheckConnectionStatus();
            if (string.IsNullOrEmpty(_selectedmailbox))
                SelectMailbox("INBOX");
        }

        public MailMessage GetMessage(string uid, bool headersonly = false) {
            return GetMessage(uid, headersonly, true);
        }

        public MailMessage GetMessage(int index, bool headersonly = false) {
            return GetMessage(index, headersonly, true);
        }

        public MailMessage GetMessage(int index, bool headersonly, bool setseen) {
            return GetMessages(index, index, headersonly, setseen).FirstOrDefault();
        }

        public MailMessage GetMessage(string uid, bool headersonly, bool setseen) {
            return GetMessages(uid, uid, headersonly, setseen).FirstOrDefault();
        }

        public MailMessage[] GetMessages(string startUID, string endUID, bool headersonly = true, bool setseen = false) {
            return GetMessages(startUID, endUID, true, headersonly, setseen);
        }

        public MailMessage[] GetMessages(int startIndex, int endIndex, bool headersonly = true, bool setseen = false) {
            return GetMessages((startIndex + 1).ToString(), (endIndex + 1).ToString(), false, headersonly, setseen);
        }

        public MailMessage[] GetMessages(string start, string end, bool uid, bool headersonly, bool setseen) {
            CheckMailboxSelected();

            string UID, HEADERS, SETSEEN;
            UID = HEADERS = SETSEEN = String.Empty;
            if (uid) UID = "UID ";
            if (headersonly) HEADERS = "HEADER";
            if (setseen) SETSEEN = ".PEEK";
            string tag = GetTag();
            string command = tag + UID + "FETCH " + start + ":" + end + " (UID RFC822.SIZE FLAGS BODY" + SETSEEN + "[" + HEADERS + "])";
            string response = SendCommandGetResponse(command);
            var x = new List<MailMessage>();
            string reg = @"\* \d+ FETCH.*?BODY.*?\{(\d+)\}";
            Match m = Regex.Match(response, reg);
            while (m.Groups.Count > 1) {
                int bodylen = Convert.ToInt32(m.Groups[1].ToString());
                MailMessage mail = new MailMessage();
                char[] body = new char[bodylen];
                int total = 0;
                while (total < bodylen) {
                    int read = _Reader.Read(body, total, bodylen - total);
                    total += read;
                }

                string message = new string(body);

                Match m2 = Regex.Match(response, @"UID (\d+)");
                if (m2.Groups[1] != null) mail.Uid = m2.Groups[1].ToString();
                m2 = Regex.Match(response, @"FLAGS \((.*?)\)");
                if (m2.Groups[1] != null) mail.SetFlags(m2.Groups[1].ToString());
                m2 = Regex.Match(response, @"RFC822\.SIZE (\d+)");
                if (m2.Groups[1] != null) mail.Size = Convert.ToInt32(m2.Groups[1].ToString());
                mail.Load(new string(body), headersonly);
                x.Add(mail);
                response = _Reader.ReadLine(); // read last line terminated by )
                response = _Reader.ReadLine(); // read next line
                m = Regex.Match(response, reg);
            }
            return x.ToArray();
        }

        public Quota GetQuota(string mailbox) {
            CheckConnectionStatus();

            if (_capability == null) Capability();
            if (_capability.IndexOf("NAMESPACE") == -1)
                new Exception("This command is not supported by the server!");

            string command = GetTag() + "GETQUOTAROOT " + mailbox;
            string response = SendCommandGetResponse(command);
            string reg = "\\* QUOTA (.*?) \\((.*?) (.*?) (.*?)\\)";
            while (response.StartsWith("*")) {
                Match m = Regex.Match(response, reg);
                if (m.Groups.Count > 1) {
                    return new Quota(m.Groups[1].ToString(),
                                        m.Groups[2].ToString(),
                                        Int32.Parse(m.Groups[3].ToString()),
                                        Int32.Parse(m.Groups[4].ToString())
                                    );
                }
                response = _Reader.ReadLine();
            }
            return null;
        }

        public Mailbox[] ListMailboxes(string reference, string pattern) {
            CheckConnectionStatus();

            var x = new List<Mailbox>();
            string command = GetTag() + "LIST \"" + reference + "\" \"" + pattern + "\"";
            string reg = "\\* LIST \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"([^\\\"]+)\\\"";
            string response = SendCommandGetResponse(command);
            Match m = Regex.Match(response, reg);
            while (m.Groups.Count > 1) {
                Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
                x.Add(mailbox);
                response = _Reader.ReadLine();
                m = Regex.Match(response, reg);
            }
            return x.ToArray();
        }

        public Mailbox[] ListSuscribesMailboxes(string reference, string pattern) {
            CheckConnectionStatus();

            var x = new List<Mailbox>();
            string command = GetTag() + "LSUB \"" + reference + "\" \"" + pattern + "\"";
            string reg = "\\* LSUB \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"([^\\\"]+)\\\"";
            string response = SendCommandGetResponse(command);
            Match m = Regex.Match(response, reg);
            while (m.Groups.Count > 1) {
                Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
                x.Add(mailbox);
                response = _Reader.ReadLine();
                m = Regex.Match(response, reg);
            }
            return x.ToArray();
        }

        internal override void OnLogin(string login, string password) {
            string command = String.Empty;
            string result = String.Empty;
            string tag = GetTag();
            string key;

            switch (AuthMethod) {
                case AuthMethods.CRAMMD5:
                    command = tag + "AUTHENTICATE CRAM-MD5";
                    result = SendCommandGetResponse(command);
                    // retrieve server key
                    key = result.Replace("+ ", "");
                    key = System.Text.Encoding.Default.GetString(Convert.FromBase64String(key));
                    // calcul hash
                    using (HMACMD5 kMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(password))) {
                        byte[] hash1 = kMd5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(key));
                        key = BitConverter.ToString(hash1).ToLower().Replace("-", "");
                        result = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(login + " " + key));
                        result = SendCommandGetResponse(result);
                    }
                    break;

                case AuthMethods.Login:
                    command = tag + "LOGIN " + login + " " + password;
                    result = SendCommandGetResponse(command);
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (result.StartsWith("* CAPABILITY ")) {
                _capability = result.Substring(13);
                result = _Reader.ReadLine();
            }

            if (!result.StartsWith(tag + "OK")) {
                throw new Exception(result);
            }
        }

        internal override void OnLogout() {
            SendCommand(GetTag() + "LOGOUT");
        }

        public Namespaces Namespace() {
            CheckConnectionStatus();
            if (_capability == null) Capability();

            if (_capability.IndexOf("NAMESPACE") == -1) throw new NotSupportedException("This command is not supported by the server!");
            string command = GetTag() + "NAMESPACE";
            string response = SendCommandGetResponse(command);
            //Console.WriteLine(response);
            if (response.StartsWith("* NAMESPACE")) {
                response = response.Substring(12);
                Namespaces n = new Namespaces();
                //[TODO] be sure to parse correctly namespace when not all namespaces are present. NIL character
                string reg = @"\((.*?)\) \((.*?)\) \((.*?)\)$";
                Match m = Regex.Match(response, reg);
                if (m.Groups.Count != 4) throw new Exception("En error occure, this command is not fully supported !");
                string reg2 = "\\(\\\"(.*?)\\\" \\\"(.*?)\\\"\\)";
                Match m2 = Regex.Match(m.Groups[1].ToString(), reg2);
                while (m2.Groups.Count > 1) {
                    n.ServerNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
                    m2 = m2.NextMatch();
                }
                m2 = Regex.Match(m.Groups[2].ToString(), reg2);
                while (m2.Groups.Count > 1) {
                    n.UserNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
                    m2 = m2.NextMatch();
                }
                m2 = Regex.Match(m.Groups[3].ToString(), reg2);
                while (m2.Groups.Count > 1) {
                    n.SharedNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
                    m2 = m2.NextMatch();
                }
                _Reader.ReadLine();
                return n;
            } else {
                throw new Exception("Unknow server response !");
            }

        }

        public int GetMessageCount() {
            CheckMailboxSelected();
            return GetMessageCount(null);
        }
        public int GetMessageCount(string mailbox) {
            CheckConnectionStatus();

            string command = GetTag() + "STATUS " + (mailbox ?? _selectedmailbox) + " (MESSAGES)";
            string response = SendCommandGetResponse(command);
            string reg = @"\* STATUS.*MESSAGES (\d+)";
            int result = 0;
            while (response.StartsWith("*")) {
                Match m = Regex.Match(response, reg);
                if (m.Groups.Count > 1) result = Convert.ToInt32(m.Groups[1].ToString());
                response = _Reader.ReadLine();
                m = Regex.Match(response, reg);
            }
            return result;
        }

        public void RenameMailbox(string frommailbox, string tomailbox) {
            CheckConnectionStatus();

            string command = GetTag() + "RENAME \"" + frommailbox + "\" \"" + tomailbox + "\"";
            SendCommandCheckOK(command);
        }

        public string[] Search(string criteria, bool uid) {
            CheckMailboxSelected();

            string isuid = uid ? "UID " : "";
            string command = GetTag() + isuid + "SEARCH " + criteria;
            string response = SendCommandGetResponse(command);
            _Reader.DiscardBufferedData();

            string reg = @"^\* SEARCH (.*)";
            List<string> ms = new List<string>();
            Match m = Regex.Match(response, reg);
            if (m.Groups.Count > 1) {
                string[] uids = m.Groups[1].ToString().Trim().Split(' ');
                foreach (string s in uids) {
                    ms.Add(s);
                }
                return ms.ToArray();
            } else {
                throw new Exception(response);
            }
        }

        public Mailbox SelectMailbox(string mailbox) {
            CheckConnectionStatus();

            Mailbox x = null;
            string tag = GetTag();
            string command = tag + "SELECT \"" + mailbox + "\"";
            string response = SendCommandGetResponse(command);
            if (response.StartsWith("*")) {
                x = new Mailbox(mailbox);
                while (response.StartsWith("*")) {
                    Match m;
                    m = Regex.Match(response, @"(\d+) EXISTS");
                    if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
                    m = Regex.Match(response, @"(\d+) RECENT");
                    if (m.Groups.Count > 1) x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
                    m = Regex.Match(response, @"UNSEEN (\d+)");
                    if (m.Groups.Count > 1) x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
                    m = Regex.Match(response, @" FLAGS \((.*?)\)");
                    if (m.Groups.Count > 1) x.SetFlags(m.Groups[1].ToString());
                    response = _Reader.ReadLine();
                }
                if (response.StartsWith(tag + "OK")) {
                    if (response.ToUpper().IndexOf("READ/WRITE") > -1) x.Rw = true;
                }
                _selectedmailbox = mailbox;
            }
            return x;
        }

        public void Store(string messageset, bool replace, string flags) {
            CheckMailboxSelected();
            string prefix = null;
            if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
                messageset = messageset.Substring(4);
                prefix = "UID ";
            }

            string command = string.Concat(GetTag(), prefix, "STORE ", messageset, " ", replace ? "+" : "", "FLAGS.SILENT (" + flags + ")");
            string response = SendCommandGetResponse(command);
            while (response.StartsWith("*")) {
                response = _Reader.ReadLine();
            }
            CheckResultOK(response);
        }

        public void SuscribeMailbox(string mailbox) {
            CheckConnectionStatus();

            string command = GetTag() + "SUBSCRIBE \"" + mailbox + "\"";
            SendCommandCheckOK(command);
        }

        public void UnSuscribeMailbox(string mailbox) {
            CheckConnectionStatus();

            string command = GetTag() + "UNSUBSCRIBE \"" + mailbox + "\"";
            SendCommandCheckOK(command);
        }

        internal override void CheckResultOK(string response) {
            response = response.Substring(response.IndexOf(" ")).Trim();
            if (!response.ToUpper().StartsWith("OK")) {
                throw new Exception(response);
            }
        }

    }
}
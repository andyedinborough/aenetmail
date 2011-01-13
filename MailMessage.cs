using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    internal static class Extensions {
        public static MailAddress ToEmailAddress(this string input) {
            try {
                return new MailAddress(input);
            } catch (Exception) {
                return null;
            }
        }
    }

    public class MailMessage {
        private Collection<Attachment> _Attachments = new Collection<Attachment>();
        private string _Body = String.Empty;

        private Dictionary<string, string> _Headers;
        private bool _HeadersOnly; // set to true if only headers have been fetched.

        public MailMessage() {
            Flags = new string[0];
        }

        private MailAddress[] GetAddresses(string header) {
            string values = GetHeader(header).Trim();
            List<MailAddress> addrs = new List<MailAddress>();
            while (true) {
                int semicolon = values.IndexOf(';');
                int comma = values.IndexOf(',');
                if (comma < semicolon || semicolon == -1) semicolon = comma;

                int bracket = values.IndexOf('>');
                string temp = null;
                if (semicolon == -1 && bracket == -1) {
                    if (values.Length > 0) addrs.Add(values.ToEmailAddress());
                    return addrs.Where(x => x != null).ToArray();
                } else {
                    if (bracket > -1 && (semicolon == -1 || bracket < semicolon)) {
                        temp = values.Substring(0, bracket + 1);
                        values = values.Substring(temp.Length);
                    } else if (semicolon > -1 && (bracket == -1 || semicolon < bracket)) {
                        temp = values.Substring(0, semicolon);
                        values = values.Substring(semicolon + 1);
                    }
                    if (temp.Length > 0)
                        addrs.Add(temp.Trim().ToEmailAddress());
                    values = values.Trim();
                }
            }
        }

        public string Body {
            get {
                if (string.IsNullOrEmpty(_Body) && Attachments != null && Attachments.Count > 0) {
                    var att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType == "text/plain");
                    if (att != null) {
                        _Body = att.Content;
                    }
                }
                return _Body;
            }
            set { _Body = value; }
        }

        private string _BodyHtml;
        public string BodyHtml {
            get {
                if (_BodyHtml == null) {
                    var att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType.Contains("html"));
                    if (att == null)
                        _BodyHtml = string.Empty;
                    else {
                        _BodyHtml = att.Content;
                    }
                }
                return _BodyHtml;
            }
        }

        public DateTime Date { get; private set; }
        public string[] Flags { get; private set; }

        public string RawHeaders { get; private set; }
        public int Size { get; internal set; }
        public string Subject { get; private set; }
        public MailAddress[] To { get; private set; }
        public MailAddress[] Cc { get; private set; }
        public MailAddress[] Bcc { get; private set; }
        public MailAddress From { get; private set; }
        public MailAddress ReplyTo { get; private set; }
        public MailAddress Sender { get; private set; }
        public string MessageID { get; private set; }
        public string Uid { get; internal set; }
        public string Raw { get; private set; }
        public MailPriority Importance { get; private set; }

        public Collection<Attachment> Attachments {
            get { return _Attachments; }
        }

        private static Regex rxBoundary = new Regex("boundary=[\"](.*?)[\"]\\r\\n", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string GetBoundary(string messagepart) {
            return rxBoundary.Match(messagepart).Groups[1].Value;
        }

        public static Dictionary<string, string> ParseHeaders(string headers) {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] lines = headers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                int i = line.IndexOf(':');
                if (i > -1) {
                    string key = line.Substring(0, i).Trim();
                    string value = line.Substring(i + 1).Trim();
                    if (result.ContainsKey(key))
                        result[key] = value;
                    else result.Add(key, value);
                }
            }
            return result;
        }

        public string GetHeader(string Name) {
            string value;
            if (_Headers.TryGetValue(Name, out value)) return value;
            else return string.Empty;
        }

        public void Load(string message, bool headersonly) {
            Raw = message;
            _HeadersOnly = headersonly;
            if (headersonly) {
                RawHeaders = message;
            } else {
                StringBuilder headers = new StringBuilder();
                using (StringReader reader = new System.IO.StringReader(message)) {
                    string line;
                    do {
                        line = reader.ReadLine();
                        headers.AppendLine(line);
                    } while (line != string.Empty);
                    RawHeaders = headers.ToString();

                    string boundary = GetBoundary(RawHeaders);
                    if (boundary == String.Empty) {
                        _Body = reader.ReadToEnd();

                    } else { //else this is a multipart Mime Message
                        ParseMime(reader, boundary);
                    }
                }
            }

            _Headers = ParseHeaders(RawHeaders);

            Date = DateTime.Parse(GetHeader("Date"));
            To = GetAddresses("To");
            Cc = GetAddresses("Cc");
            Bcc = GetAddresses("Bcc");
            Sender = GetAddresses("Sender").FirstOrDefault();
            ReplyTo = GetAddresses("Reply-To").FirstOrDefault();
            From = GetAddresses("From").FirstOrDefault();
            MessageID = GetHeader("Message-ID");

            Importance = GetEnum<MailPriority>(GetHeader("Importance"));
            Subject = GetHeader("Subject");
        }

        private static T GetEnum<T>(string name) where T : struct, IConvertible {
            var values = System.Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            return values.FirstOrDefault(x => string.Compare(name, x.ToString(), true) == 0);
        }

        private void ParseMime(StringReader reader, string boundary) {
            string data = reader.ReadLine();
            string bounderInner = string.Concat("--", boundary);
            string bounderOuter = string.Concat(bounderInner, "--");

            do {
                data = reader.ReadLine();
            } while (!data.StartsWith("--" + boundary));

            while (!data.StartsWith(bounderOuter)) {
                data = reader.ReadLine();
                Attachment a = new Attachment();

                StringBuilder part = new StringBuilder();
                // read part header
                while (!data.StartsWith(bounderInner) && data != string.Empty) {
                    part.AppendLine(data);
                    data = reader.ReadLine();
                }
                a.Header = part.ToString();
                // header body

                data = reader.ReadLine();
                var body = new StringBuilder();
                while (!data.StartsWith(bounderInner)) {
                    body.AppendLine(data);
                    data = reader.ReadLine();
                }
                // check for nested part
                string nestedboundary = GetBoundary(a.Header);
                if (nestedboundary == String.Empty) {
                    a.Content = body.ToString();
                    this._Attachments.Add(a);

                } else { // nested
                    ParseMime(new System.IO.StringReader(body.ToString()), nestedboundary);
                }
            }
        }

        internal void SetFlags(string flags) {
            Flags = flags.Split(' ');
        }
    }
}
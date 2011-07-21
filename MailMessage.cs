using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AE.Net.Mail {
    public class MailMessage : ObjectWHeaders {
        private string _Body = null;
        private bool _HeadersOnly; // set to true if only headers have been fetched.

        public MailMessage() {
            Flags = new string[0];
            Attachments = new Collection<Attachment>();
        }

        public string Body {
            get {
                if (_Body == null && Attachments != null && Attachments.Count > 0) {
                    var att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType == "text/plain");
                    if (att != null) {
                        _Body = att.Content;
                        if (_Body.LooksLikeHtml()) {
                            _BodyHtml = _Body;
                            _Body = _Body.StripHtml();
                        }
                    }
                }
                return _Body;
            }
            set { _Body = value ?? string.Empty; }
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

        public ICollection<Attachment> Attachments { get; private set; }

        public void Load(string message, bool headersonly) {
            Raw = message;
            _HeadersOnly = headersonly;
            if (headersonly) {
                RawHeaders = message;
            } else {
                var headers = new StringBuilder();
                using (var reader = new System.IO.StringReader(message)) {
                    string line;
                    do {
                        line = reader.ReadLine();
                        headers.AppendLine(line);
                    } while (line != string.Empty);
                    RawHeaders = headers.ToString();

                    string boundary = Headers.GetBoundary();
                    if (!string.IsNullOrEmpty(boundary)) {
                        //else this is a multipart Mime Message
                        ParseMime(reader, boundary);
                    } else {
                        _Body = reader.ReadToEnd();
                    }
                }
            }

            Date = Headers.GetDate("Date");
            To = Headers.GetAddresses("To");
            Cc = Headers.GetAddresses("Cc");
            Bcc = Headers.GetAddresses("Bcc");
            Sender = Headers.GetAddresses("Sender").FirstOrDefault();
            ReplyTo = Headers.GetAddresses("Reply-To").FirstOrDefault();
            From = Headers.GetAddresses("From").FirstOrDefault();
            MessageID = Headers["Message-ID"].RawValue;

            Importance = Headers.GetEnum<MailPriority>("Importance");
            Subject = Headers["Subject"].RawValue;
        }

        private void ParseMime(StringReader reader, string boundary) {
            string data = reader.ReadLine(),
                bounderInner = "--" + boundary,
                bounderOuter = bounderInner + "--";

            do {
                data = reader.ReadLine();
            } while (!data.StartsWith("--" + boundary));

            while (data != null && !data.StartsWith(bounderOuter)) {
                data = reader.ReadLine();
                var a = new Attachment();

                var part = new StringBuilder();
                // read part header
                while (!data.StartsWith(bounderInner) && data != string.Empty) {
                    part.AppendLine(data);
                    data = reader.ReadLine();
                }
                a.RawHeaders = part.ToString();
                // header body

                data = reader.ReadLine();
                var body = new StringBuilder();
                while (data != null && !data.StartsWith(bounderInner)) {
                    body.AppendLine(data);
                    data = reader.ReadLine();
                }
                // check for nested part
                string nestedboundary = a.Headers.GetBoundary();
                if (!string.IsNullOrEmpty(nestedboundary)) {
                    ParseMime(new System.IO.StringReader(body.ToString()), nestedboundary);

                } else { // nested
                    a.Content = body.ToString();
                    Attachments.Add(a);
                }
            }
        }

        internal void SetFlags(string flags) {
            Flags = flags.Split(' ');
        }
    }
}
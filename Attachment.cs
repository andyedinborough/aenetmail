/*
 * Attachment.cs
 * Copyright (C) 2006 COLIN Cyrille.
 *
 */

using System;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    public class Attachment {
        private string _header = string.Empty;
        private string _content = string.Empty;
        private bool _onserver;

        public string Filename {
            get { return GetHeader("name="); }
        }

        private string Charset {
            get { return GetHeader("charset="); }
        }

        private string ContentDisposition {
            get { return GetHeader("Content-Disposition: "); }
        }

        public string ContentEncoding {
            get { return GetHeader("Content-Transfer-Encoding: "); }
        }

        public string ContentType {
            get { return GetHeader("Content-Type: "); }
        }

        public bool IsAttachment {
            get {
                return (ContentDisposition.ToLower() == "attachment" || ContentDisposition.ToLower() == "inline") ? true : false;
            }
        }

        public void Save(string filename) {
            using (var file = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                Save(file);
        }

        public void Save(System.IO.Stream stream) {
            var data = GetContent();
            stream.Write(data, 0, data.Length);
        }

        public byte[] GetContent() {
            byte[] data;
            if (ContentEncoding == "base64") {
                data = Convert.FromBase64String(Content);
            } else {
                data = System.Text.Encoding.Default.GetBytes(Content);
            }
            return data;
        }

        public string Content {
            get { return this._content; }
            internal set {
                if (ContentEncoding == "quoted-printable") {
                    value = Regex.Replace(value, @"\=[\r\n]+", string.Empty, RegexOptions.Singleline);
                    var matches = Regex.Matches(value, @"\=[0-9A-F]{2}");
                    foreach (Match match in matches) {
                        int ascii = int.Parse(match.Value.Substring(1), System.Globalization.NumberStyles.HexNumber);
                        char c = Convert.ToChar(ascii);
                        value = value.Replace(match.Value, c.ToString());
                    }
                }
                this._content = value;
            }
        }

        public string Header {
            get { return this._header; }
            internal set { this._header = value; }
        }

        public bool OnServer {
            get { return this._onserver; }
            internal set { this._onserver = value; }
        }

        private string GetHeader(string header) {
            Match m;
            m = Regex.Match(this._header, header + "[\"]?(.*?)[\"]?(\\r\\n|;)", RegexOptions.Multiline);
            if (m.Groups.Count > 1) {
                return m.Groups[1].ToString();
            } else {
                return String.Empty;
            }
        }
    }
}
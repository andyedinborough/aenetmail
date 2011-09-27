using System;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
    public class Attachment : ObjectWHeaders {
        private string _content = string.Empty;

        public string Filename {
            get { return Headers["Content-Disposition"]["filename"]; }
        }

        private string Charset {
            get { return Headers["Content-Transfer-Encoding"]["charset"]; }
        }

        private string _ContentDisposition;
        private string ContentDisposition {
            get { return _ContentDisposition ?? (_ContentDisposition = Headers["Content-Disposition"].Value.ToLower()); }
        }

        public string ContentEncoding {
            get { return Headers["Content-Transfer-Encoding"].Value ?? string.Empty; }
        }

        public string ContentType {
            get { return Headers["Content-Type"].Value ?? string.Empty; }
        }

        public bool OnServer { get; internal set; }

        public bool IsAttachment {
            get {
                return ContentDisposition == "attachment" || ContentDisposition == "inline";
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
                try {
                    data = Convert.FromBase64String(_content);
                } catch (Exception) {
                    data = System.Text.Encoding.UTF8.GetBytes(_content);
                }
            } else {
                data = System.Text.Encoding.UTF8.GetBytes(_content);
            }
            return data;
        }

        public string Content {
            get { return _content; }
            internal set {
                if (ContentEncoding.Is("quoted-printable")) {
                    value = Regex.Replace(value, @"\=[\r\n]+", string.Empty, RegexOptions.Singleline);
                    var matches = Regex.Matches(value, @"\=[0-9A-F]{2}");
                    foreach (Match match in matches) {
                        int ascii = int.Parse(match.Value.Substring(1), System.Globalization.NumberStyles.HexNumber);
                        char c = Convert.ToChar(ascii);
                        value = value.Replace(match.Value, c.ToString());
                    }
                } else if (ContentEncoding.Is("base64") && ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) {
                    value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                _content = value;
            }
        }
    }
}
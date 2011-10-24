using System;

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
            internal set {
                Headers["Content-Transfer-Encoding"] = new HeaderValue(value);
            }
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
            if (ContentEncoding.Is("base64") && Utilities.IsValidBase64String(_content)) {
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
                    value = Utilities.DecodeQuotedPrintable(value, Utilities.ParseCharsetToEncoding(Charset));

                } else if (ContentEncoding.Is("base64")
                    //only decode the content if it is a text document
                        && ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                        && Utilities.IsValidBase64String(_content)) {
                    value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                    ContentEncoding = string.Empty;
                }

                _content = value;
            }
        }
    }
}
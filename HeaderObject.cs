
using System;
namespace AE.Net.Mail {
    public abstract class ObjectWHeaders {
        public string RawHeaders { get; internal set; }
        private HeaderCollection _Headers;
        public HeaderCollection Headers {
            get {
                return _Headers ?? (_Headers = HeaderCollection.Parse(RawHeaders));
            }
        }

        public string ContentTransferEncoding {
            get { return Headers["Content-Transfer-Encoding"].Value ?? string.Empty; }
            internal set {
                Headers["Content-Transfer-Encoding"] = new HeaderValue(value);
            }
        }

        public string ContentType {
            get { return Headers["Content-Type"].Value ?? string.Empty; }
        }

        public string Charset {
            get { return Headers["Content-Transfer-Encoding"]["charset"]; }
        }

        public string Body { get; set; }

        internal void SetBody(string value) {
            if (ContentTransferEncoding.Is("quoted-printable")) {
                value = Utilities.DecodeQuotedPrintable(value, Utilities.ParseCharsetToEncoding(Charset));

            } else if (ContentTransferEncoding.Is("base64")
                //only decode the content if it is a text document
                    && ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                    && Utilities.IsValidBase64String(value)) {
                value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
                ContentTransferEncoding = string.Empty;
            }

            Body = value;
        }

    }
}


using System;
namespace AE.Net.Mail {
	public abstract class ObjectWHeaders {
		public virtual string RawHeaders { get; internal set; }
		private HeaderDictionary _Headers;
		public virtual HeaderDictionary Headers {
			get {
				return _Headers ?? (_Headers = HeaderDictionary.Parse(RawHeaders, _DefaultEncoding));
			}
			internal set {
				_Headers = value;
			}
		}

		public virtual string ContentTransferEncoding {
			get { return Headers["Content-Transfer-Encoding"].Value ?? string.Empty; }
			set {
				Headers.Set("Content-Transfer-Encoding", new HeaderValue(value));
			}
		}

		public virtual string ContentType {
			get { return Headers["Content-Type"].Value.NotEmpty("text/plain"); }
			set {
				Headers.Set("Content-Type", new HeaderValue(value));
			}
		}

		public virtual string Charset {
			get {
				return Headers["Content-Transfer-Encoding"]["charset"].NotEmpty(
					Headers["Content-Type"]["charset"]
				);
			}
		}

		protected System.Text.Encoding _DefaultEncoding = System.Text.Encoding.GetEncoding(1252);
		protected System.Text.Encoding _Encoding;
		public virtual System.Text.Encoding Encoding {
			get {
				return _Encoding ?? (_Encoding = Utilities.ParseCharsetToEncoding(Charset, _DefaultEncoding));
			}
			set {
				_DefaultEncoding = value ?? _DefaultEncoding;
				if (_Encoding != null) //Encoding has been initialized from the specified Charset
					_Encoding = value ?? _DefaultEncoding;
			}
		}

		public virtual string Body { get; set; }

		internal void SetBody(string value) {
			if (ContentTransferEncoding.Is("quoted-printable")) {
				value = Utilities.DecodeQuotedPrintable(value, Encoding);

			} else if (ContentTransferEncoding.Is("base64")
				//only decode the content if it is a text document
							&& ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
							&& Utilities.IsValidBase64String(ref value)) {
				var data = Convert.FromBase64String(value);
				using (var mem = new System.IO.MemoryStream(data))
				using (var str = new System.IO.StreamReader(mem, Encoding))
					value = str.ReadToEnd();

				ContentTransferEncoding = string.Empty;
			}

			Body = value;
		}

		internal void SetBody(byte[] data) {
			ContentTransferEncoding = "base64";
			Body = Convert.ToBase64String(data);
		}
	}
}

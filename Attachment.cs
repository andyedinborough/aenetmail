using System;

namespace AE.Net.Mail {
	public class Attachment : ObjectWHeaders {
		public virtual string Filename {
			get
			{
			    return Headers["Content-Disposition"]["filename"].NotEmpty(
                            Headers["Content-Disposition"]["name"],
                            Headers["Content-Type"]["filename"],
                            Headers["Content-Type"]["name"]);
			}
		}

		private string _ContentDisposition;
		private string ContentDisposition {
			get { return _ContentDisposition ?? (_ContentDisposition = Headers["Content-Disposition"].Value.ToLower()); }
		}

		public virtual bool OnServer { get; internal set; }

		internal bool IsAttachment {
			get {
				return ContentDisposition == "attachment" || !string.IsNullOrEmpty(Filename);
			}
		}

		public virtual void Save(string filename) {
			using (var file = new System.IO.FileStream(filename, System.IO.FileMode.Create))
				Save(file);
		}

		public virtual void Save(System.IO.Stream stream) {
			var data = GetData();
			stream.Write(data, 0, data.Length);
		}

		public virtual byte[] GetData() {
			byte[] data;
			if (ContentTransferEncoding.Is("base64") && Utilities.IsValidBase64String(Body)) {
				try {
					data = Convert.FromBase64String(Body);
				} catch (Exception) {
					data = Encoding.GetBytes(Body);
				}
			} else {
				data = Encoding.GetBytes(Body);
			}
			return data;
		}

	}
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AE.Net.Mail {
	public class AlternateViewCollection : Collection<Attachment> {
		/// <summary>
		/// Find views matching a specific content-type.
		/// </summary>
		/// <param name="contentType">The content-type to search for; such as "text/html"</param>
		/// <returns></returns>
		public IEnumerable<Attachment> OfType(string contentType) {
			contentType = (contentType ?? string.Empty).ToLower();
			return OfType(x => x.Is(contentType));
		}

		/// <summary>
		/// Find views where the content-type matches a condition
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public IEnumerable<Attachment> OfType(Func<string, bool> predicate) {
			return this.Where(x => predicate((x.ContentType ?? string.Empty).Trim()));
		}

		public Attachment GetHtmlView() {
			return OfType("text/html").FirstOrDefault() ?? OfType(ct => ct.Contains("html")).FirstOrDefault();
		}

		public Attachment GetTextView() {
			return OfType("text/plain").FirstOrDefault() ?? OfType(ct => ct.StartsWith("text/")).FirstOrDefault();
		}
	}
}

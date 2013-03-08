using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Tests {
	[TestClass]
	public class AttachmentTests {
		[TestMethod]
		public void AttachmentType_ContentDispositionAttachment_Attachment() {
			var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("attachment") } } };

			attachment.ContentDisposition.Inline.ShouldNotBe();
		}

		[TestMethod]
		public void AttachmentType_ContentDispositionAttachmentWithFileName_Attachment() {
			var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"attachment; filename=""test.html""") } } };

			attachment.ContentDisposition.Inline.ShouldNotBe();
			attachment.ContentDisposition.FileName.ShouldBe("test.html");
		}

		[TestMethod]
		public void AttachmentType_ContentDispositionInline_Inline() {
			var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("inline") } } };

			attachment.ContentDisposition.Inline.ShouldBe();
		}

		[TestMethod]
		public void AttachmentType_ContentDispositionInlineWithFileName_Inline() {
			var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"inline; filename=""test.html""") } } };

			attachment.ContentDisposition.Inline.ShouldBe();
			attachment.ContentDisposition.FileName.ShouldBe("test.html");
		}

		[TestMethod]
		public void AttachmentType_ContentDispositionUnknown_Unknown() {
			var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("rubbish") } } };

			attachment.ContentDisposition.DispositionType.ShouldBe("rubbish");
		}

		[TestMethod]
		public void AttachmentType_NoContentDisposition_Unknown() {
			var attachment = new Attachment { Headers = new HeaderDictionary() };

			attachment.ContentDisposition.Inline.ShouldBe();
		}
	}
}
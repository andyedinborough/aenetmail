using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class AttachmentTests
    {
        [TestMethod]
        public void AttachmentType_ContentDispositionAttachment_Attachment()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("attachment") } } };

            Assert.AreEqual(AttachmentType.Attachment, attachment.AttachmentType);
        }

        [TestMethod]
        public void AttachmentType_ContentDispositionAttachmentWithFileName_Attachment()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"attachment; filename=""test.html""") } } };

            Assert.AreEqual(AttachmentType.Attachment, attachment.AttachmentType);
        }

        [TestMethod]
        public void AttachmentType_ContentDispositionInline_Inline()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("inline") } } };

            Assert.AreEqual(AttachmentType.Inline, attachment.AttachmentType);
        }

        [TestMethod]
        public void AttachmentType_ContentDispositionInlineWithFileName_Inline()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"inline; filename=""test.html""") } } };

            Assert.AreEqual(AttachmentType.Inline, attachment.AttachmentType);
        }

        [TestMethod]
        public void AttachmentType_ContentDispositionUnknown_Unknown()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("rubbish") } } };

            Assert.AreEqual(AttachmentType.Unknown, attachment.AttachmentType);
        }

        [TestMethod]
        public void AttachmentType_NoContentDisposition_Unknown()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary() };

            Assert.AreEqual(AttachmentType.Unknown, attachment.AttachmentType);
        }
    }
}

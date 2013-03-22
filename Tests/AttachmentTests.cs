using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Tests
{
    [TestClass]
    public class AttachmentTests
    {
        [TestMethod]
        public void Inline_ContentDispositionAttachment_False()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("attachment") } } };

            attachment.ContentDisposition.Inline.ShouldNotBe();
        }

        [TestMethod]
        public void Inline_ContentDispositionAttachmentWithFileName_False()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"attachment; filename=""test.html""") } } };

            attachment.ContentDisposition.Inline.ShouldNotBe();
            attachment.ContentDisposition.FileName.ShouldBe("test.html");
        }

        [TestMethod]
        public void Inline_ContentDispositionInline_True()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("inline") } } };

            attachment.ContentDisposition.Inline.ShouldBe();
        }

        [TestMethod]
        public void Inline_ContentDispositionInlineWithFileName_True()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"inline; filename=""test.html""") } } };

            attachment.ContentDisposition.Inline.ShouldBe();
            attachment.ContentDisposition.FileName.ShouldBe("test.html");
        }

        [TestMethod]
        public void Inline_ContentDispositionUnknown_False()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue("rubbish") } } };

            attachment.ContentDisposition.Inline.ShouldNotBe();
        }

        [TestMethod]
        public void Inline_NoContentDisposition_True()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary() };

            attachment.ContentDisposition.Inline.ShouldBe();
        }

        [TestMethod]
        public void Inline_ContentDispositionInlineWithFileNameWithExoticCharacter_True()
        {
            var attachment = new Attachment { Headers = new HeaderDictionary { { "Content-Disposition", new HeaderValue(@"attachment;filename=""2013135 Charité.pdf""") } } };

            attachment.ContentDisposition.Inline.ShouldNotBe();
            attachment.ContentDisposition.FileName.ShouldBe("2013135 Charite.pdf");
        }
    }
}
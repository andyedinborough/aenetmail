using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AE.Net.Mail.Imap;


using Shouldly;
using Xunit;

namespace Tests
{
    public class ModifiedUtf7EncodingTest {

        #region Methods

        [Fact]
        public void Decode_AsciiOnlyInput_ReturnsOriginalString() {
            string mailboxNameWithAsciiOnly = "Sent";

            string result = ModifiedUtf7Encoding.Decode(mailboxNameWithAsciiOnly);

            result.ShouldBe(mailboxNameWithAsciiOnly);
        }

        [Fact]
        public void Decode_InputNull_ReturnsNull()
        {
            ModifiedUtf7Encoding.Decode(null).ShouldBe(null);
        }

        [Fact]
        public void Decode_InputWithConventionalAmpersand_ReturnsStringWithDecodedAmpersand()
        {
            string mailboxWithAmpersand = "Test &- Test";

            string result = ModifiedUtf7Encoding.Decode(mailboxWithAmpersand);

            result.ShouldBe("Test & Test");
        }

        [Fact]
        public void Decode_InputWithEncodedCyrillicCharacters_ReturnsDecodedCyrillicCharacters()
        {
            string mailboxNameWithCyrillicCharacters = "&BB4EQgQ,BEAEMAQyBDsENQQ9BD0ESwQ1-";

            string result = ModifiedUtf7Encoding.Decode(mailboxNameWithCyrillicCharacters);

            result.ShouldBe("Отправленные");
        }

        [Fact]
        public void Decode_InputWithEncodedUmlaut_ReturnsStringWithDecodedUmlaut() {
            string mailboxNameWithUmlaut = "Entw&APw-rfe";

            string result = ModifiedUtf7Encoding.Decode(mailboxNameWithUmlaut);

            result.ShouldBe("Entwürfe");
        }
        [Fact]
        public void Encode_AsciiOnlyInput_ReturnsOriginalString() {
            string mailboxNameWithAsciiOnly = "Sent";

            string result = ModifiedUtf7Encoding.Encode(mailboxNameWithAsciiOnly);

            result.ShouldBe(mailboxNameWithAsciiOnly);
        }

        [Fact]
        public void Encode_InputNull_ReturnsNull()
        {
            ModifiedUtf7Encoding.Encode(null).ShouldBe(null);
        }

        [Fact]
        public void Encode_InputWithConventionalAmpersand_ReturnsStringWithAmpersandMinus()
        {
            string mailboxWithAmpersand = "Test & Test";

            string result = ModifiedUtf7Encoding.Encode(mailboxWithAmpersand);

            result.ShouldBe("Test &- Test");
        }

        [Fact]
        public void Encode_InputWithCyrillicCharacters_ReturnsStringWithEncodedCyrillicCharacters()
        {
            string mailboxNameWithCyrillicCharacters = "Отправленные";

            string result = ModifiedUtf7Encoding.Encode(mailboxNameWithCyrillicCharacters);

            result.ShouldBe("&BB4EQgQ,BEAEMAQyBDsENQQ9BD0ESwQ1-");
        }

        [Fact]
        public void Encode_InputWithUmlaut_ReturnsStringWithEncodedUmlaut() {
            string mailboxNameWithUmlaut = "Entwürfe";

            string result = ModifiedUtf7Encoding.Encode(mailboxNameWithUmlaut);

            result.ShouldBe("Entw&APw-rfe");
        }

        #endregion
    }
}

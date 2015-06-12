using System.Linq;
using AE.Net.Mail;
using Xunit;

namespace Tests
{
    public class HeaderDictionaryTests
    {
        [Fact]
        public void GetAddresses_EmptyHeader_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_Rubbish_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"<<<<,783459@")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_OnlyOneSemicolon_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@";")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_OnlyOneComma_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@",")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_FiveCommasOnly_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@",,,,,")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_Spaces_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"    ")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(0, mailAddresses.Length);
        }

        [Fact]
        public void GetAddresses_SimpleWithQuotesAroundDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"""name"" <name@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.Equal("name@domain.net", mailAddress.Address);
            Assert.Equal("name", mailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_SimpleWithoutQuotesAroundDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"Test van Testenstein <test@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.Equal("test@domain.net", mailAddress.Address);
            Assert.Equal("Test van Testenstein", mailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_PipelineInDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"Test | Testenstein <test@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.Equal("test@domain.net", mailAddress.Address);
            Assert.Equal("Test | Testenstein", mailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_WithCommaInDispayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"""lastname, firstname"" <firstname.lastname@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.Equal(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.Equal("firstname.lastname@domain.net", mailAddress.Address);
            Assert.Equal("lastname, firstname", mailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_WithTwoAddresses_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, second.address@domain.net")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.Equal(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.Equal("first@domain.net", firstMailAddress.Address);
            Assert.Equal("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.Equal("second.address@domain.net", secondMailAddress.Address);
            Assert.Equal("", secondMailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_WithTwoAddressesWithCommaInSecondEmail_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, ""Test, 2"" <second.address@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.Equal(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.Equal("first@domain.net", firstMailAddress.Address);
            Assert.Equal("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.Equal("second.address@domain.net", secondMailAddress.Address);
            Assert.Equal("Test, 2", secondMailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_WithTwoAddressesWithCommaInSecondEmailAndTrailingComma_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, ""Test, 2"" <second.address@domain.net>, ")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.Equal(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.Equal("first@domain.net", firstMailAddress.Address);
            Assert.Equal("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.Equal("second.address@domain.net", secondMailAddress.Address);
            Assert.Equal("Test, 2", secondMailAddress.DisplayName);
        }

        [Fact]
        public void GetAddresses_WithThreeAddresses_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"<test1@domain.net>,   <test2@domain.net>,   <test3@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.Equal(3, mailAddresses.Length);

            var firstMailAddress = mailAddresses.ElementAt(0);
            Assert.Equal("test1@domain.net", firstMailAddress.Address);
            Assert.Equal("", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.ElementAt(1);
            Assert.Equal("test2@domain.net", secondMailAddress.Address);
            Assert.Equal("", secondMailAddress.DisplayName);

            var thirdMailAddress = mailAddresses.ElementAt(2);
            Assert.Equal("test3@domain.net", thirdMailAddress.Address);
            Assert.Equal("", thirdMailAddress.DisplayName);
        }
    }
}

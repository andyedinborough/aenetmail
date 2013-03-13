using System.Linq;
using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class HeaderDictionaryTests
    {
        [TestMethod]
        public void GetAddresses_EmptyHeader_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_Rubbish_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"<<<<,783459@")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_OnlyOneSemicolon_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@";")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_OnlyOneComma_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@",")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_FiveCommasOnly_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@",,,,,")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_Spaces_ReturnsNoEmailAddresses()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"    ")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(0, mailAddresses.Length);
        }

        [TestMethod]
        public void GetAddresses_SimpleWithQuotesAroundDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"""name"" <name@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.AreEqual("name@domain.net", mailAddress.Address);
            Assert.AreEqual("name", mailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_SimpleWithoutQuotesAroundDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"Test van Testenstein <test@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.AreEqual("test@domain.net", mailAddress.Address);
            Assert.AreEqual("Test van Testenstein", mailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_PipelineInDisplayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"Test | Testenstein <test@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.AreEqual("test@domain.net", mailAddress.Address);
            Assert.AreEqual("Test | Testenstein", mailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_WithCommaInDispayName_ParsesCorrectAddressAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "From", new HeaderValue(@"""lastname, firstname"" <firstname.lastname@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("From");

            Assert.AreEqual(1, mailAddresses.Length);

            var mailAddress = mailAddresses.First();
            Assert.AreEqual("firstname.lastname@domain.net", mailAddress.Address);
            Assert.AreEqual("lastname, firstname", mailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_WithTwoAddresses_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, second.address@domain.net")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.AreEqual(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.AreEqual("first@domain.net", firstMailAddress.Address);
            Assert.AreEqual("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.AreEqual("second.address@domain.net", secondMailAddress.Address);
            Assert.AreEqual("", secondMailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_WithTwoAddressesWithCommaInSecondEmail_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, ""Test, 2"" <second.address@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.AreEqual(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.AreEqual("first@domain.net", firstMailAddress.Address);
            Assert.AreEqual("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.AreEqual("second.address@domain.net", secondMailAddress.Address);
            Assert.AreEqual("Test, 2", secondMailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_WithTwoAddressesWithCommaInSecondEmailAndTrailingComma_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"Firstname Lastname <first@domain.net>, ""Test, 2"" <second.address@domain.net>, ")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.AreEqual(2, mailAddresses.Length);

            var firstMailAddress = mailAddresses.First();
            Assert.AreEqual("first@domain.net", firstMailAddress.Address);
            Assert.AreEqual("Firstname Lastname", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.Last();
            Assert.AreEqual("second.address@domain.net", secondMailAddress.Address);
            Assert.AreEqual("Test, 2", secondMailAddress.DisplayName);
        }

        [TestMethod]
        public void GetAddresses_WithThreeAddresses_ParsesCorrectAddressesAndDisplayName()
        {
            var headerDictionary = new HeaderDictionary
                {
                    { "To", new HeaderValue(@"<test1@domain.net>,   <test2@domain.net>,   <test3@domain.net>")}
                };

            var mailAddresses = headerDictionary.GetMailAddresses("To");

            Assert.AreEqual(3, mailAddresses.Length);

            var firstMailAddress = mailAddresses.ElementAt(0);
            Assert.AreEqual("test1@domain.net", firstMailAddress.Address);
            Assert.AreEqual("", firstMailAddress.DisplayName);

            var secondMailAddress = mailAddresses.ElementAt(1);
            Assert.AreEqual("test2@domain.net", secondMailAddress.Address);
            Assert.AreEqual("", secondMailAddress.DisplayName);

            var thirdMailAddress = mailAddresses.ElementAt(2);
            Assert.AreEqual("test3@domain.net", thirdMailAddress.Address);
            Assert.AreEqual("", thirdMailAddress.DisplayName);
        }
    }
}

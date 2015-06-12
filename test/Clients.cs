using AE.Net.Mail;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using System.Reflection;

namespace Tests {
	public class Clients {

		[Fact]
		public void Search_Conditions() {
			var deleted = SearchCondition.Deleted();
			var seen = SearchCondition.Seen();
			var text = SearchCondition.Text("andy");

			deleted.ToString().ShouldBe("DELETED");
			deleted.Or(seen).ToString().ShouldBe("OR (DELETED) (SEEN)");
			seen.And(text).ToString().ShouldBe("(SEEN) (TEXT \"andy\")");

			var since = new DateTime(2000, 1, 1);
			SearchCondition.Undeleted().And(
									SearchCondition.From("david"),
									SearchCondition.SentSince(since)
							).Or(SearchCondition.To("andy"))
					.ToString()
					.ShouldBe("OR ((UNDELETED) (FROM \"david\") (SENTSINCE \"" + Utilities.GetRFC2060Date(since) + "\")) (TO \"andy\")");
        }

        [Fact]
        public void Parse_Imap_Header()
        {
            var header = @"X-GM-THRID 1320777376118077475 X-GM-MSGID 1320777376118077475 X-GM-LABELS () UID 8286 RFC822.SIZE 9369 FLAGS (\Seen) BODY[] {9369}";

            var values = Utilities.ParseImapHeader(header);
            values["FLAGS"].ShouldBe(@"\Seen");
            values["UID"].ShouldBe("8286");
            values["X-GM-MSGID"].ShouldBe("1320777376118077475");
            values["X-GM-LABELS"].ShouldBeNullOrEmpty();
            values["RFC822.SIZE"].ShouldBe("9369");
        }

        [Fact]
        public void Email_Addresses()
        {
            "andy@localhost".ToEmailAddress().ShouldBe();
            "andy.edinborough@localhost.com".ToEmailAddress().Host.ShouldBe("localhost.com");
            "Andy <andy@localhost>".ToEmailAddress().DisplayName.ShouldBe("Andy");
            "'Andy' <andy@localhost>".ToEmailAddress().DisplayName.ShouldBe("Andy");
            @"""Andy's"" <andy@localhost>".ToEmailAddress().DisplayName.ShouldBe("Andy's");
            @"""Andy's"" <andy.edinborough@localhost>".ToEmailAddress().User.ShouldBe("andy.edinborough");
        }

        public static string GetSolutionDirectory() {
			var dir = new System.IO.DirectoryInfo(string.Empty);
			while (dir.GetFiles("*.sln").Length == 0) {
				dir = dir.Parent;
			}
			return dir.FullName;
		}
	}
}

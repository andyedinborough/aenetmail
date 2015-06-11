using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests {
	/// <summary>
	/// Spam downloaded from http://www.untroubled.org/spam/
	/// </summary>
	[TestClass]
	public class ParseSpam {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void Parse_Spam() {
			var dir = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "AE.Net.Mail" }, StringSplitOptions.RemoveEmptyEntries)[0],
				"AE.Net.Mail\\Tests\\Spam");
			var files = System.IO.Directory.GetFiles(dir, "*.lorien", System.IO.SearchOption.AllDirectories);

			var mindate = new DateTime(1900, 1, 1).Ticks;
			var maxdate = DateTime.MaxValue.Ticks;
			var rxSubject = new Regex(@"^Subject\:\s+\S+");
			MailMessage msg = new MailMessage();
			for (var i = 0; i < files.Length; i++) {
				var file = files[i];
				var txt = System.IO.File.ReadAllText(file);
				using (var stream = System.IO.File.OpenRead(file))
					msg.Load(stream, false, (int)stream.Length);

				if (msg.ContentTransferEncoding.IndexOf("quoted", StringComparison.OrdinalIgnoreCase) == -1) {
					continue;
				}

				if (string.IsNullOrEmpty(msg.Body)) {
					continue;
				}

				try {

					msg.Date.Ticks.ShouldBeInRange(mindate, maxdate);
					if (string.IsNullOrEmpty(msg.Subject) && rxSubject.IsMatch(txt))
						throw new AssertFailedException("subject is null or empty");
					//msg.From.ShouldBe();
					if (msg.To.Count > 0) msg.To.First().ShouldBe();
					if (msg.Cc.Count > 0) msg.Cc.First().ShouldBe();
					if (msg.Bcc.Count > 0) msg.Bcc.First().ShouldBe();

					(msg.Body ?? string.Empty).Trim().ShouldNotBeNullOrEmpty();


				} catch (Exception ex) {
					Console.WriteLine(ex);
					Console.WriteLine(txt);
					throw;
				}
			}
		}
	}
}

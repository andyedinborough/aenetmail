using System;
using System.Text.RegularExpressions;
using AE.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should.Fluent;
using System.Linq;

namespace Tests {
  /// <summary>
  /// Spam downloaded from http://www.untroubled.org/spam/
  /// </summary>
  [TestClass]
  public class ParseSpam {
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestParsing() {
      var dir = System.IO.Path.Combine(Environment.CurrentDirectory.Split(new[] { "AE.Net.Mail" }, StringSplitOptions.RemoveEmptyEntries)[0],
        "AE.Net.Mail\\Tests\\Spam");
      var files = System.IO.Directory.GetFiles(dir, "*.lorien", System.IO.SearchOption.AllDirectories);

      var mindate = new DateTime(1900, 1, 1).Ticks;
      var maxdate = DateTime.MaxValue.Ticks;
      var rxSubject = new Regex(@"^Subject\:\s+\S+");
      MailMessage msg = new MailMessage();
      for (var i = 0; i < files.Length; i++) {
        var file = files[i];
        msg.Load(System.IO.File.ReadAllText(file), false);
        if (msg.ContentTransferEncoding.IndexOf("quoted", StringComparison.OrdinalIgnoreCase) == -1) {
          continue;
        }

        if (string.IsNullOrEmpty(msg.Body)) {
          continue;
        }

        try {

          msg.Date.Ticks.Should().Be.InRange(mindate, maxdate);
          if (string.IsNullOrEmpty(msg.Subject) && rxSubject.IsMatch(msg.Raw))
            throw new AssertFailedException("subject is null or empty");
          //msg.From.Should().Not.Be.Null();
          if (msg.To.Count > 0) msg.To.First().Should().Not.Be.Null();
          if (msg.Cc.Count > 0) msg.Cc.First().Should().Not.Be.Null();
          if (msg.Bcc.Count > 0) msg.Bcc.First().Should().Not.Be.Null();

          (msg.Body ?? string.Empty).Trim().Should().Not.Be.NullOrEmpty();


        } catch (Exception ex) {
          Console.WriteLine(ex);
          Console.WriteLine(msg.Raw);
          throw;
        }
      }
    }
  }
}

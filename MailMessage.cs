using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AE.Net.Mail {
  public enum MailPriority {
    Normal = 3, High = 5, Low = 1
  }

  [System.Flags]
  public enum Flags {
    None = 0,
    Seen = 1,
    Answered = 2,
    Flagged = 4,
    Deleted = 8,
    Draft = 16
  }

  public class MailMessage : ObjectWHeaders {
    private bool _HeadersOnly; // set to true if only headers have been fetched. 

    public MailMessage() {
      RawFlags = new string[0];
      Attachments = new Collection<Attachment>();
    }

    public DateTime Date { get; private set; }
    public string[] RawFlags { get; private set; }
    public Flags Flags { get; private set; }

    public int Size { get; internal set; }
    public string Subject { get; private set; }
    public MailAddress[] To { get; private set; }
    public MailAddress[] Cc { get; private set; }
    public MailAddress[] Bcc { get; private set; }
    public MailAddress From { get; private set; }
    public MailAddress ReplyTo { get; private set; }
    public MailAddress Sender { get; private set; }
    public string MessageID { get; private set; }
    public string Uid { get; internal set; }
    public string Raw { get; private set; }
    public MailPriority Importance { get; private set; }

    public ICollection<Attachment> Attachments { get; private set; }

    public void Load(string message, bool headersOnly = false) {
      Raw = message;
      using (var reader = new StringReader(message)) {
        Load(reader, headersOnly);
      }
    }

    public void Load(TextReader reader, bool headersOnly = false) {
      _HeadersOnly = headersOnly;
      Headers = null;
      Body = null;

      if (headersOnly) {
        RawHeaders = reader.ReadToEnd();
      } else {
        var headers = new StringBuilder();
        string line;
        while ((line = reader.ReadLine()) != null) {
          if (line.Trim().Length == 0)
            if (headers.Length == 0)
              continue;
            else break;
          headers.AppendLine(line);
        }
        RawHeaders = headers.ToString();

        string boundary = Headers.GetBoundary();
        if (!string.IsNullOrEmpty(boundary)) {
          //else this is a multipart Mime Message
          using (var subreader = new StringReader(line + Environment.NewLine + reader.ReadToEnd()))
            ParseMime(subreader, boundary);
        } else {
          SetBody((line + Environment.NewLine + reader.ReadToEnd()).Trim());
        }

        if (string.IsNullOrEmpty(Body) && Attachments != null && Attachments.Count > 0) {
          var att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType.Is("text/plain"));
          if (att == null) {
            att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType.Contains("html"));
          }

          if (att != null) {
            Body = att.Body;
            ContentTransferEncoding = att.Headers["Content-Transfer-Encoding"].RawValue;
            ContentType = att.Headers["Content-Type"].RawValue;
          }
        }
      }

      Date = Headers.GetDate();
      To = Headers.GetAddresses("To");
      Cc = Headers.GetAddresses("Cc");
      Bcc = Headers.GetAddresses("Bcc");
      Sender = Headers.GetAddresses("Sender").FirstOrDefault();
      ReplyTo = Headers.GetAddresses("Reply-To").FirstOrDefault();
      From = Headers.GetAddresses("From").FirstOrDefault();
      MessageID = Headers["Message-ID"].RawValue;

      Importance = Headers.GetEnum<MailPriority>("Importance");
      Subject = Headers["Subject"].RawValue;
    }

    [Obsolete("Use Body instead--check content-type to determine if it's HTML.  If HTML is needed, find an attachment in GetBodyAttachments() with a text/html content-type."), EditorBrowsable(EditorBrowsableState.Never)]
    public string BodyHtml {
      get {
        if (ContentType.Contains("html")) return Body;
        return GetBodyAttachments()
          .Where(x => x.ContentType.Contains("html"))
          .Select(x => x.Body)
          .FirstOrDefault();
      }
    }

    public IEnumerable<Attachment> GetBodyAttachments() {
      return Attachments.Where(x => !x.IsAttachment);
    }

    private void ParseMime(TextReader reader, string boundary) {
      string data,
        bounderInner = "--" + boundary,
        bounderOuter = bounderInner + "--";

      do {
        data = reader.ReadLine();
      } while (data != null && !data.StartsWith(bounderInner));

      while (data != null && !data.StartsWith(bounderOuter)) {
        data = reader.ReadLine();
        var a = new Attachment();

        var part = new StringBuilder();
        // read part header
        while (!data.StartsWith(bounderInner) && data != string.Empty) {
          part.AppendLine(data);
          data = reader.ReadLine();
        }
        a.RawHeaders = part.ToString();
        // header body

        data = reader.ReadLine();
        var body = new StringBuilder();
        while (data != null && !data.StartsWith(bounderInner)) {
          body.AppendLine(data);
          data = reader.ReadLine();
        }
        // check for nested part
        string nestedboundary = a.Headers.GetBoundary();
        if (!string.IsNullOrEmpty(nestedboundary)) {
          ParseMime(new System.IO.StringReader(body.ToString()), nestedboundary);

        } else { // nested
          a.SetBody(body.ToString());
          Attachments.Add(a);
        }
      }
    }

    private static Dictionary<string, int> _FlagCache = System.Enum.GetValues(typeof(Flags)).Cast<Flags>().ToDictionary(x => x.ToString(), x => (int)x, StringComparer.OrdinalIgnoreCase);
    internal void SetFlags(string flags) {
      RawFlags = flags.Split(' ').Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
      Flags = (Flags)RawFlags.Select(x => {
        int flag = 0;
        if (_FlagCache.TryGetValue(x.TrimStart('\\'), out flag))
          return flag;
        else return 0;
      }).Sum();
    }
  }
}
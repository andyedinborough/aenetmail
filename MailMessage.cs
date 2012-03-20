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
    Normal = 3,
    High = 5,
    Low = 1
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
    public static implicit operator System.Net.Mail.MailMessage(MailMessage msg) {
      var ret = new System.Net.Mail.MailMessage();
      ret.Subject = msg.Subject;
      ret.Sender = msg.Sender;
      foreach (var a in msg.Bcc)
        ret.Bcc.Add(a);
      ret.Body = msg.Body;
      ret.IsBodyHtml = msg.ContentType.Contains("html");
      ret.From = msg.From;
      ret.Priority = (System.Net.Mail.MailPriority)msg.Importance;
      foreach (var a in msg.ReplyTo)
        ret.ReplyToList.Add(a);
      foreach (var a in msg.To)
        ret.To.Add(a);
      foreach (var a in msg.Attachments)
        ret.Attachments.Add(new System.Net.Mail.Attachment(new System.IO.MemoryStream(a.GetData()), a.Filename, a.ContentType));
      foreach (var a in msg.AlternateViews)
        ret.AlternateViews.Add(new System.Net.Mail.AlternateView(new System.IO.MemoryStream(a.GetData()), a.ContentType));

      return ret;
    }

    private bool _HeadersOnly; // set to true if only headers have been fetched. 

    public MailMessage() {
      RawFlags = new string[0];
      To = new List<MailAddress>();
      Cc = new List<MailAddress>();
      Bcc = new List<MailAddress>();
      ReplyTo = new List<MailAddress>();
      Attachments = new List<Attachment>();
      AlternateViews = new List<Attachment>();
    }

    public DateTime Date { get; set; }
    public string[] RawFlags { get; set; }
    public Flags Flags { get; set; }

    public int Size { get; internal set; }
    public string Subject { get; set; }
    public ICollection<MailAddress> To { get; private set; }
    public ICollection<MailAddress> Cc { get; private set; }
    public ICollection<MailAddress> Bcc { get; private set; }
    public ICollection<MailAddress> ReplyTo { get; private set; }
    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<Attachment> AlternateViews { get; set; }
    public MailAddress From { get; set; }
    public MailAddress Sender { get; set; }
    public string MessageID { get; set; }
    public string Uid { get; internal set; }
    public string Raw { get; private set; }
    public MailPriority Importance { get; set; }


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
            else
              break;
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
      }

      if (string.IsNullOrWhiteSpace(Body) && AlternateViews.Count > 0) {
        var att = AlternateViews.FirstOrDefault(x => x.ContentType.Is("text/plain"));
        if (att == null) {
          att = AlternateViews.FirstOrDefault(x => x.ContentType.Contains("html"));
        }

        if (att != null) {
          Body = att.Body;
          ContentTransferEncoding = att.Headers["Content-Transfer-Encoding"].RawValue;
          ContentType = att.Headers["Content-Type"].RawValue;
        }
      }

      Date = Headers.GetDate();
      To = Headers.GetAddresses("To").ToList();
      Cc = Headers.GetAddresses("Cc").ToList();
      Bcc = Headers.GetAddresses("Bcc").ToList();
      Sender = Headers.GetAddresses("Sender").FirstOrDefault();
      ReplyTo = Headers.GetAddresses("Reply-To").ToList();
      From = Headers.GetAddresses("From").FirstOrDefault();
      MessageID = Headers["Message-ID"].RawValue;

      Importance = Headers.GetEnum<MailPriority>("Importance");
      Subject = Headers["Subject"].RawValue;
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
          using (var nestedReader = new System.IO.StringReader(body.ToString()))
            ParseMime(nestedReader, nestedboundary);

        } else { // nested
          a.SetBody(body.ToString());
          (a.IsAttachment ? Attachments : AlternateViews).Add(a);
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
        else
          return 0;
      }).Sum();
    }

    public void Save(System.IO.Stream stream, Encoding encoding = null) {
      using (var str = new System.IO.StreamWriter(stream, encoding ?? System.Text.Encoding.Default))
        Save(str);
    }

    private static readonly string[] SpecialHeaders = "Date,To,Cc,Reply-To,Bcc,Sender,From,Message-ID,Importance,Subject".Split(',');
    public void Save(System.IO.TextWriter txt) {
      txt.WriteLine("Date: {0}", Date.GetRFC2060Date());
      txt.WriteLine("To: ", string.Join("; ", To.Select(x => x.ToString())));
      txt.WriteLine("Cc: ", string.Join("; ", Cc.Select(x => x.ToString())));
      txt.WriteLine("Reply-To: ", string.Join("; ", ReplyTo.Select(x => x.ToString())));
      txt.WriteLine("Bcc: ", string.Join("; ", Bcc.Select(x => x.ToString())));
      if (Sender != null)
        txt.WriteLine("Sender: ", Sender);
      if (From != null)
        txt.WriteLine("From: ", Sender);
      if (!string.IsNullOrEmpty(MessageID))
        txt.WriteLine("Message-ID: ", MessageID);

      var otherHeaders = Headers.Where(x => !SpecialHeaders.Contains(x.Key, StringComparer.InvariantCultureIgnoreCase));
      foreach (var header in otherHeaders) {
        txt.WriteLine("{0}: {1}", header.Key, header.Value);
      }
      if (Importance != MailPriority.Normal)
        txt.WriteLine("Importance: {0}", (int)Importance);
      txt.WriteLine("Subject: {0}", Subject);
      txt.WriteLine();

      //todo: attachments
      txt.Write(Body);
    }
  }
}
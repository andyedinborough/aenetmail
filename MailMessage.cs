using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace AE.Net.Mail {
  public enum MailPriority {
    Normal = 3, High = 5, Low = 1
  }

  public class MailMessage : ObjectWHeaders {
    private bool _HeadersOnly; // set to true if only headers have been fetched.

    public MailMessage() {
      Flags = new string[0];
      Attachments = new Collection<Attachment>();
    }

    public string BodyHtml { get; set; }

    public DateTime Date { get; private set; }
    public string[] Flags { get; private set; }

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

    public void Load(string message, bool headersonly) {
      Raw = message;
      using (var reader = new StringReader(message)) {
        Load(reader, headersonly);
      }
    }

    private Regex rxHeader = new Regex(@"^[a-z\-]+\:\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public void Load(TextReader reader, bool headersonly) {
      var peekable = new PeekableTextReader(reader);
      _HeadersOnly = headersonly;
      Headers = null;
      Body = BodyHtml = null;

      if (headersonly) {
        RawHeaders = peekable.ReadToEnd();
      } else {
        var headers = new StringBuilder();
        string line;
        while (true) {
          line = peekable.ReadLine();
          if (string.IsNullOrEmpty(line)) {
            if (rxHeader.IsMatch(line = (peekable.PeekLine() ?? string.Empty)) && string.IsNullOrEmpty(peekable.PeekLine())) {
              //this line is a header! keep going!
            } else {
              break;
            }
          } else {
            headers.AppendLine(line);
          }
        }
        RawHeaders = headers.ToString();

        string boundary = Headers.GetBoundary();
        if (!string.IsNullOrEmpty(boundary)) {
          //else this is a multipart Mime Message
          ParseMime(peekable, boundary);
        } else {
          SetBody((peekable.ReadToEnd() ?? string.Empty).Trim());
        }

        if (Attachments != null && Attachments.Count > 0) {
          Attachment att;
          if (string.IsNullOrEmpty(Body)) {
            att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType.Is("text/plain"));
            if (att != null) {
              Body = att.Body;
            } else {
              Body = string.Empty;
            }
          }

          att = Attachments.FirstOrDefault(x => !x.IsAttachment && x.ContentType.Contains("html"));
          if (att != null) {
            BodyHtml = att.Body;
          } else {
            BodyHtml = string.Empty;
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

    private void ParseMime(TextReader reader, string boundary) {
      string data = reader.ReadLine(),
          bounderInner = "--" + boundary,
          bounderOuter = bounderInner + "--";

      do {
        data = reader.ReadLine();
      } while (data != null && !data.StartsWith("--" + boundary));

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

    internal void SetFlags(string flags) {
      Flags = flags.Split(' ');
    }
  }
}
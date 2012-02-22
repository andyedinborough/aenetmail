
using System;
namespace AE.Net.Mail {
  public abstract class ObjectWHeaders {
    public string RawHeaders { get; internal set; }
    private HeaderDictionary _Headers;
    public HeaderDictionary Headers {
      get {
        return _Headers ?? (_Headers = HeaderDictionary.Parse(RawHeaders));
      }
      internal set {
        _Headers = value;
      }
    }

    public string ContentTransferEncoding {
      get { return Headers["Content-Transfer-Encoding"].Value ?? string.Empty; }
      internal set {
        Headers.Set("Content-Transfer-Encoding", new HeaderValue(value));
      }
    }

    public string ContentType {
      get { return Headers["Content-Type"].Value ?? string.Empty; }
      internal set {
        Headers.Set("Content-Type", new HeaderValue(value));
      }
    }

    public string Charset {
      get {
        return Headers["Content-Transfer-Encoding"]["charset"].NotEmpty(
        Headers["Content-Type"]["charset"]
        );
      }
    }

    public string Body { get; set; }

    internal void SetBody(string value) {
      if (ContentTransferEncoding.Is("quoted-printable")) {
        value = Utilities.DecodeQuotedPrintable(value, Utilities.ParseCharsetToEncoding(Charset));

      } else if (ContentTransferEncoding.Is("base64")
        //only decode the content if it is a text document
              && ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
              && Utilities.IsValidBase64String(value)) {
        var data = Convert.FromBase64String(value);
        using (var mem = new System.IO.MemoryStream(data))
        using (var str = new System.IO.StreamReader(mem, true))
          value = str.ReadToEnd();

        ContentTransferEncoding = string.Empty;
      }

      Body = value;
    }
  }
}

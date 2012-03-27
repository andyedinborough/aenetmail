using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AE.Net.Mail.Imap;

namespace AE.Net.Mail {

  public class ImapClient : TextClient, IMailClient {
    private string _SelectedMailbox;
    private int _tag = 0;
    private string[] _Capability;

    private bool _Idling;
    private Thread _IdleEvents;

    private string _FetchHeaders = null;

    public ImapClient() { }
    public ImapClient(string host, string username, string password, AuthMethods method = AuthMethods.Login, int port = 143, bool secure = false, bool skipSslValidation = false) {
      Connect(host, port, secure, skipSslValidation);
      AuthMethod = method;
      Login(username, password);
    }

    public enum AuthMethods {
      Login,
      CRAMMD5
    }

    public AuthMethods AuthMethod { get; set; }

    private string GetTag() {
      _tag++;
      return string.Format("xm{0:000} ", _tag);
    }

    public bool Supports(string command) {
      return (_Capability ?? Capability()).Contains(command, StringComparer.OrdinalIgnoreCase);
    }

    private EventHandler<MessageEventArgs> _NewMessage;
    public event EventHandler<MessageEventArgs> NewMessage {
      add {
        _NewMessage += value;
        IdleStart();
      }
      remove {
        _NewMessage -= value;
        if (!HasEvents)
          IdleStop();
      }
    }

    private EventHandler<MessageEventArgs> _MessageDeleted;
    public event EventHandler<MessageEventArgs> MessageDeleted {
      add {
        _MessageDeleted += value;
        IdleStart();
      }
      remove {
        _MessageDeleted -= value;
        if (!HasEvents)
          IdleStop();
      }
    }

    private void IdleStart() {
      if (string.IsNullOrEmpty(_SelectedMailbox)) {
        SelectMailbox("Inbox");
      }
      _Idling = true;
      if (!Supports("IDLE")) {
        throw new InvalidOperationException("This IMAP server does not support the IDLE command");
      }
      CheckMailboxSelected();
      IdleResume();
    }

    private void IdlePause() {
      if (_IdleEvents == null || !_Idling)
        return;

      CheckConnectionStatus();
      SendCommand("DONE");
      if (!_IdleEvents.Join(2000))
        _IdleEvents.Abort();
      _IdleEvents = null;
    }

    private void IdleResume() {
      if (!_Idling)
        return;

      IdleResumeCommand();

      if (_IdleEvents == null) {
        _IdleEvents = new Thread(WatchIdleQueue);
        _IdleEvents.Name = "_IdleEvents";
        _IdleEvents.Start();
      }
    }

    private void IdleResumeCommand() {
      SendCommandGetResponse(GetTag() + "IDLE");
      _IdleARE.Set();
    }

    private bool HasEvents {
      get {
        return _MessageDeleted != null || _NewMessage != null;
      }
    }

    private void IdleStop() {
      _Idling = false;
      IdlePause();
      if (_IdleEvents != null) {
        _IdleARE.Close();
        if (!_IdleEvents.Join(2000))
          _IdleEvents.Abort();
        _IdleEvents = null;
      }
    }

    public bool TryGetResponse(out string response, int millisecondsTimeout) {
      var mre = new System.Threading.ManualResetEventSlim(false);
      string resp = response = null;
      ThreadPool.QueueUserWorkItem(_ => {
        resp = GetResponse();
        mre.Set();
      });

      if (mre.Wait(millisecondsTimeout)) {
        response = resp;
        return true;
      } else
        return false;
    }

    private static readonly int idleTimeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
    private static AutoResetEvent _IdleARE = new AutoResetEvent(false);
    private void WatchIdleQueue() {
      try {
        string last = null, resp;

        while (true) {
          if (!TryGetResponse(out resp, idleTimeout)) {   //send NOOP every 20 minutes
            Noop(false);        //call noop without aborting this Idle thread
            continue;
          }

          if (resp.Contains("OK IDLE"))
            return;

          var data = resp.Split(' ');
          if (data[0] == "*" && data.Length >= 3) {
            var e = new MessageEventArgs { Client = this, MessageCount = int.Parse(data[1]) };
            if (data[2].Is("EXISTS") && !last.Is("EXPUNGE") && e.MessageCount > 0) {
              ThreadPool.QueueUserWorkItem(callback => _NewMessage.Fire(this, e));    //Fire the event on a separate thread
            } else if (data[2].Is("EXPUNGE")) {
              _MessageDeleted.Fire(this, e);
            }
            last = data[2];
          }
        }
      } catch (Exception) { }
    }

    protected override void OnDispose() {
      base.OnDispose();
      if (_IdleEvents != null) {
        _IdleEvents.Abort();
        _IdleEvents = null;
      }
    }

    public void AppendMail(MailMessage email, string mailbox = null) {
      IdlePause();

      string flags = String.Empty;
      var body = new StringBuilder();
      using (var txt = new System.IO.StringWriter(body))
        email.Save(txt);

      string size = body.Length.ToString();
      if (email.RawFlags.Length > 0) {
        flags = " (" + string.Join(" ", email.Flags) + ")";
      }

      if (mailbox == null)
        CheckMailboxSelected();
      mailbox = mailbox ?? _SelectedMailbox;

      string command = GetTag() + "APPEND " + (mailbox ?? _SelectedMailbox).QuoteString() + flags + " {" + size + "}";
      string response = SendCommandGetResponse(command);
      if (response.StartsWith("+")) {
        response = SendCommandGetResponse(body.ToString());
      }
      IdleResume();
    }

    public void Noop() {
      Noop(true);
    }
    private void Noop(bool pauseIdle) {
      if (pauseIdle)
        IdlePause();
      else
        SendCommandGetResponse("DONE");

      var tag = GetTag();
      var response = SendCommandGetResponse(tag + "NOOP");
      while (!response.StartsWith(tag)) {
        response = GetResponse();
      }

      if (pauseIdle)
        IdleResume();
      else
        IdleResumeCommand();
    }

    public string[] Capability() {
      IdlePause();
      string command = GetTag() + "CAPABILITY";
      string response = SendCommandGetResponse(command);
      if (response.StartsWith("* CAPABILITY "))
        response = response.Substring(13);
      _Capability = response.Trim().Split(' ');
      GetResponse();
      IdleResume();
      return _Capability;
    }

    public void Copy(string messageset, string destination) {
      CheckMailboxSelected();
      IdlePause();
      string prefix = null;
      if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
        messageset = messageset.Substring(4);
        prefix = "UID ";
      }
      string command = string.Concat(GetTag(), prefix, "COPY ", messageset, " " + destination.QuoteString());
      SendCommandCheckOK(command);
      IdleResume();
    }

    public void CreateMailbox(string mailbox) {
      IdlePause();
      string command = GetTag() + "CREATE " + mailbox.QuoteString();
      SendCommandCheckOK(command);
      IdleResume();
    }

    public void DeleteMailbox(string mailbox) {
      IdlePause();
      string command = GetTag() + "DELETE " + mailbox.QuoteString();
      SendCommandCheckOK(command);
      IdleResume();
    }

    public Mailbox Examine(string mailbox) {
      IdlePause();

      Mailbox x = null;
      string tag = GetTag();
      string command = tag + "EXAMINE " + mailbox.QuoteString();
      string response = SendCommandGetResponse(command);
      if (response.StartsWith("*")) {
        x = new Mailbox(mailbox);
        while (response.StartsWith("*")) {
          Match m;
          m = Regex.Match(response, @"(\d+) EXISTS");
          if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
          m = Regex.Match(response, @"(\d+) RECENT");
          if (m.Groups.Count > 1)
            x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
          m = Regex.Match(response, @"UNSEEN (\d+)");
          if (m.Groups.Count > 1)
            x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
          m = Regex.Match(response, @" FLAGS \((.*?)\)");
          if (m.Groups.Count > 1)
            x.SetFlags(m.Groups[1].ToString());
          response = GetResponse();
        }
        _SelectedMailbox = mailbox;
      }
      IdleResume();
      return x;
    }

    public void Expunge() {
      CheckMailboxSelected();
      IdlePause();

      string tag = GetTag();
      string command = tag + "EXPUNGE";
      string response = SendCommandGetResponse(command);
      while (response.StartsWith("*")) {
        response = GetResponse();
      }
      IdleResume();
    }

    public void DeleteMessage(AE.Net.Mail.MailMessage msg) {
      DeleteMessage(msg.Uid);
    }

    public void DeleteMessage(string uid) {
      CheckMailboxSelected();
      Store("UID " + uid, true, "\\Seen \\Deleted");
    }

    public void MoveMessage(string uid, string folderName) {
      CheckMailboxSelected();
      Copy("UID " + uid, folderName);
      DeleteMessage(uid);
    }

    private void CheckMailboxSelected() {
      if (string.IsNullOrEmpty(_SelectedMailbox))
        SelectMailbox("INBOX");
    }

    public MailMessage GetMessage(string uid, bool headersonly = false) {
      return GetMessage(uid, headersonly, true);
    }

    public MailMessage GetMessage(int index, bool headersonly = false) {
      return GetMessage(index, headersonly, true);
    }

    public MailMessage GetMessage(int index, bool headersonly, bool setseen) {
      return GetMessages(index, index, headersonly, setseen).FirstOrDefault();
    }

    public MailMessage GetMessage(string uid, bool headersonly, bool setseen) {
      return GetMessages(uid, uid, headersonly, setseen).FirstOrDefault();
    }

    public MailMessage[] GetMessages(string startUID, string endUID, bool headersonly = true, bool setseen = false) {
      return GetMessages(startUID, endUID, true, headersonly, setseen);
    }

    public MailMessage[] GetMessages(int startIndex, int endIndex, bool headersonly = true, bool setseen = false) {
      return GetMessages((startIndex + 1).ToString(), (endIndex + 1).ToString(), false, headersonly, setseen);
    }

    internal static NameValueCollection ParseImapHeader(string data) {
      var values = new NameValueCollection();
      string name = null;
      int nump = 0;
      var temp = new StringBuilder();
      if (data != null)
        foreach (var c in data) {
          if (c == ' ') {
            if (name == null) {
              name = temp.ToString();
              temp.Clear();

            } else if (nump == 0) {
              values[name] = temp.ToString();
              name = null;
              temp.Clear();
            } else
              temp.Append(c);
          } else if (c == '(') {
            if (nump > 0)
              temp.Append(c);
            nump++;
          } else if (c == ')') {
            nump--;
            if (nump > 0)
              temp.Append(c);
          } else
            temp.Append(c);
        }

      if (name != null)
        values[name] = temp.ToString();

      return values;
    }

    public MailMessage[] GetMessages(string start, string end, bool uid, bool headersonly, bool setseen) {
      CheckMailboxSelected();
      IdlePause();

      string tag = GetTag();
      string command = tag + (uid ? "UID " : null)
        + "FETCH " + start + ":" + end + " ("
        + _FetchHeaders + "UID FLAGS BODY"
        + (setseen ? ".PEEK" : null)
        + "[" + (headersonly ? "HEADER" : null) + "])";

      string response;
      var x = new List<MailMessage>();

      SendCommand(command);
      while (true) {
        response = GetResponse();
        if (string.IsNullOrEmpty(response) || response.Contains(tag + "OK"))
          break;

        if (response[0] != '*' || !response.Contains("FETCH ("))
          continue;

        var mail = new MailMessage();
        var imapHeaders = ParseImapHeader(response.Substring(response.IndexOf('(') + 1));
        mail.Size = (imapHeaders["BODY[HEADER]"] ?? imapHeaders["BODY[]"]).Trim('{', '}').ToInt();

        if (imapHeaders["UID"] != null)
          mail.Uid = imapHeaders["UID"];

        if (imapHeaders["Flags"] != null)
          mail.SetFlags(imapHeaders["Flags"]);


        foreach (var key in imapHeaders.AllKeys.Except(new[] { "UID", "Flags", "BODY[]", "BODY[HEADER]" }, StringComparer.OrdinalIgnoreCase))
          mail.Headers.Add(key, new HeaderValue(imapHeaders[key]));

        var body = new StringBuilder();
        int remaining = mail.Size;
        var buffer = new byte[8192];
        int read;
        while (remaining > 0) {
          read = _Stream.Read(buffer, 0, Math.Min(remaining, buffer.Length));
          body.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, read));
          remaining -= read;
        }

        var next = Convert.ToChar(_Stream.ReadByte());
        System.Diagnostics.Debug.Assert(next == ')');

        mail.Load(body.ToString(), headersonly);

        x.Add(mail);
      }

      IdleResume();
      return x.ToArray();
    }

    public Quota GetQuota(string mailbox) {
      if (!Supports("NAMESPACE"))
        new Exception("This command is not supported by the server!");
      IdlePause();

      Quota quota = null;
      string command = GetTag() + "GETQUOTAROOT " + mailbox.QuoteString();
      string response = SendCommandGetResponse(command);
      string reg = "\\* QUOTA (.*?) \\((.*?) (.*?) (.*?)\\)";
      while (response.StartsWith("*")) {
        Match m = Regex.Match(response, reg);
        if (m.Groups.Count > 1) {
          quota = new Quota(m.Groups[1].ToString(),
                              m.Groups[2].ToString(),
                              Int32.Parse(m.Groups[3].ToString()),
                              Int32.Parse(m.Groups[4].ToString())
                          );
          break;
        }
        response = GetResponse();
      }

      IdleResume();
      return quota;
    }

    public Mailbox[] ListMailboxes(string reference, string pattern) {
      IdlePause();

      var x = new List<Mailbox>();
      string command = GetTag() + "LIST " + reference.QuoteString() + " " + pattern.QuoteString();
      string reg = "\\* LIST \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"?([^\\\"]+)\\\"?";
      string response = SendCommandGetResponse(command);
      Match m = Regex.Match(response, reg);
      while (m.Groups.Count > 1) {
        Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
        x.Add(mailbox);
        response = GetResponse();
        m = Regex.Match(response, reg);
      }
      IdleResume();
      return x.ToArray();
    }

    public Mailbox[] ListSuscribesMailboxes(string reference, string pattern) {
      IdlePause();

      var x = new List<Mailbox>();
      string command = GetTag() + "LSUB " + reference.QuoteString() + " " + pattern.QuoteString();
      string reg = "\\* LSUB \\(([^\\)]*)\\) \\\"([^\\\"]+)\\\" \\\"([^\\\"]+)\\\"";
      string response = SendCommandGetResponse(command);
      Match m = Regex.Match(response, reg);
      while (m.Groups.Count > 1) {
        Mailbox mailbox = new Mailbox(m.Groups[3].ToString());
        x.Add(mailbox);
        response = GetResponse();
        m = Regex.Match(response, reg);
      }
      IdleResume();
      return x.ToArray();
    }

    internal override void OnLogin(string login, string password) {
      string command = String.Empty;
      string result = String.Empty;
      string tag = GetTag();
      string key;

      switch (AuthMethod) {
        case AuthMethods.CRAMMD5:
          command = tag + "AUTHENTICATE CRAM-MD5";
          result = SendCommandGetResponse(command);
          // retrieve server key
          key = result.Replace("+ ", "");
          key = System.Text.Encoding.Default.GetString(Convert.FromBase64String(key));
          // calcul hash
          using (var kMd5 = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(password))) {
            byte[] hash1 = kMd5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(key));
            key = BitConverter.ToString(hash1).ToLower().Replace("-", "");
            result = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(login + " " + key));
            result = SendCommandGetResponse(result);
          }
          break;

        case AuthMethods.Login:
          command = tag + "LOGIN " + login.QuoteString() + " " + password.QuoteString();
          result = SendCommandGetResponse(command);
          break;

        default:
          throw new NotSupportedException();
      }

      if (result.StartsWith("* CAPABILITY ")) {
        _Capability = result.Substring(13).Trim().Split(' ');
        result = GetResponse();
      }

      if (!result.StartsWith(tag + "OK")) {
        throw new Exception(result);
      }

      //if (Supports("COMPRESS=DEFLATE")) {
      //  SendCommandCheckOK(GetTag() + "compress deflate");
      //  _Stream0 = _Stream;
      // // _Reader = new System.IO.StreamReader(new System.IO.Compression.DeflateStream(_Stream0, System.IO.Compression.CompressionMode.Decompress, true), System.Text.Encoding.Default);
      // // _Stream = new System.IO.Compression.DeflateStream(_Stream0, System.IO.Compression.CompressionMode.Compress, true);
      //}

      if (Supports("X-GM-EXT-1")) {
        _FetchHeaders = "X-GM-MSGID X-GM-THRID X-GM-LABELS ";
      }
    }

    internal override void OnLogout() {
      if (IsConnected)
        SendCommand(GetTag() + "LOGOUT");
    }

    public Namespaces Namespace() {
      if (!Supports("NAMESPACE"))
        throw new NotSupportedException("This command is not supported by the server!");
      IdlePause();

      string command = GetTag() + "NAMESPACE";
      string response = SendCommandGetResponse(command);

      if (!response.StartsWith("* NAMESPACE")) {
        throw new Exception("Unknow server response !");
      }

      response = response.Substring(12);
      Namespaces n = new Namespaces();
      //[TODO] be sure to parse correctly namespace when not all namespaces are present. NIL character
      string reg = @"\((.*?)\) \((.*?)\) \((.*?)\)$";
      Match m = Regex.Match(response, reg);
      if (m.Groups.Count != 4)
        throw new Exception("En error occure, this command is not fully supported !");
      string reg2 = "\\(\\\"(.*?)\\\" \\\"(.*?)\\\"\\)";
      Match m2 = Regex.Match(m.Groups[1].ToString(), reg2);
      while (m2.Groups.Count > 1) {
        n.ServerNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
        m2 = m2.NextMatch();
      }
      m2 = Regex.Match(m.Groups[2].ToString(), reg2);
      while (m2.Groups.Count > 1) {
        n.UserNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
        m2 = m2.NextMatch();
      }
      m2 = Regex.Match(m.Groups[3].ToString(), reg2);
      while (m2.Groups.Count > 1) {
        n.SharedNamespace.Add(new Namespace(m2.Groups[1].Value, m2.Groups[2].Value));
        m2 = m2.NextMatch();
      }
      GetResponse();
      IdleResume();
      return n;
    }

    public int GetMessageCount() {
      CheckMailboxSelected();
      return GetMessageCount(null);
    }
    public int GetMessageCount(string mailbox) {
      IdlePause();

      string command = GetTag() + "STATUS " + Utilities.QuoteString(mailbox ?? _SelectedMailbox) + " (MESSAGES)";
      string response = SendCommandGetResponse(command);
      string reg = @"\* STATUS.*MESSAGES (\d+)";
      int result = 0;
      while (response.StartsWith("*")) {
        Match m = Regex.Match(response, reg);
        if (m.Groups.Count > 1)
          result = Convert.ToInt32(m.Groups[1].ToString());
        response = GetResponse();
        m = Regex.Match(response, reg);
      }
      IdleResume();
      return result;
    }

    public void RenameMailbox(string frommailbox, string tomailbox) {
      IdlePause();

      string command = GetTag() + "RENAME " + frommailbox.QuoteString() + " " + tomailbox.QuoteString();
      SendCommandCheckOK(command);
      IdleResume();
    }

    public string[] Search(SearchCondition criteria, bool uid = true) {
      return Search(criteria.ToString(), uid);
    }

    public string[] Search(string criteria, bool uid = true) {
      CheckMailboxSelected();

      string isuid = uid ? "UID " : "";
      string tag = GetTag();
      string command = tag + isuid + "SEARCH " + criteria;
      string response = SendCommandGetResponse(command);

      if (!response.StartsWith("* SEARCH", StringComparison.InvariantCultureIgnoreCase) && !IsResultOK(response)) {
        throw new Exception(response);
      }

      string temp;
      while (!(temp = GetResponse()).StartsWith(tag)) {
        response += Environment.NewLine + temp;
      }

      var m = Regex.Match(response, @"^\* SEARCH (.*)");
      return m.Groups[1].Value.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }

    public Lazy<MailMessage>[] SearchMessages(SearchCondition criteria, bool headersonly = false) {
      return Search(criteria, true)
          .Select(x => new Lazy<MailMessage>(() => GetMessage(x, headersonly)))
          .ToArray();
    }

    public Mailbox SelectMailbox(string mailbox) {
      IdlePause();

      Mailbox x = null;
      string tag = GetTag();
      string command = tag + "SELECT " + mailbox.QuoteString();
      string response = SendCommandGetResponse(command);
      if (response.StartsWith("*")) {
        x = new Mailbox(mailbox);
        while (response.StartsWith("*")) {
          Match m;
          m = Regex.Match(response, @"(\d+) EXISTS");
          if (m.Groups.Count > 1) { x.NumMsg = Convert.ToInt32(m.Groups[1].ToString()); }
          m = Regex.Match(response, @"(\d+) RECENT");
          if (m.Groups.Count > 1)
            x.NumNewMsg = Convert.ToInt32(m.Groups[1].ToString());
          m = Regex.Match(response, @"UNSEEN (\d+)");
          if (m.Groups.Count > 1)
            x.NumUnSeen = Convert.ToInt32(m.Groups[1].ToString());
          m = Regex.Match(response, @" FLAGS \((.*?)\)");
          if (m.Groups.Count > 1)
            x.SetFlags(m.Groups[1].ToString());
          response = GetResponse();
        }
        if (IsResultOK(response)) {
          x.IsWritable = Regex.IsMatch(response, "READ.WRITE", RegexOptions.IgnoreCase);
        }
        _SelectedMailbox = mailbox;
      } else {
        throw new Exception(response);
      }
      IdleResume();
      return x;
    }

    public void SetFlags(Flags flags, params MailMessage[] msgs) {
      SetFlags(string.Join(" ", flags.ToString().Split(',').Select(x => "\\" + x.Trim())), msgs);
    }

    public void SetFlags(string flags, params MailMessage[] msgs) {
      Store("UID " + string.Join(" ", msgs.Select(x => x.Uid)), true, flags);
      foreach (var msg in msgs) {
        msg.SetFlags(flags);
      }
    }

    public void AddFlags(Flags flags, params MailMessage[] msgs) {
      AddFlags(string.Join(" ", flags.ToString().Split(',').Select(x => "\\" + x.Trim())), msgs);
    }

    public void AddFlags(string flags, params MailMessage[] msgs) {
      Store("UID " + string.Join(" ", msgs.Select(x => x.Uid)), false, flags);
      foreach (var msg in msgs) {
        msg.SetFlags(string.Join(" ", msg.Flags) + flags);
      }
    }

    public void Store(string messageset, bool replace, string flags) {
      CheckMailboxSelected();
      IdlePause();
      string prefix = null;
      if (messageset.StartsWith("UID ", StringComparison.OrdinalIgnoreCase)) {
        messageset = messageset.Substring(4);
        prefix = "UID ";
      }

      string command = string.Concat(GetTag(), prefix, "STORE ", messageset, " ", replace ? "" : "+", "FLAGS.SILENT (" + flags + ")");
      string response = SendCommandGetResponse(command);
      while (response.StartsWith("*")) {
        response = GetResponse();
      }
      CheckResultOK(response);
      IdleResume();
    }

    public void SuscribeMailbox(string mailbox) {
      IdlePause();

      string command = GetTag() + "SUBSCRIBE " + mailbox.QuoteString();
      SendCommandCheckOK(command);
      IdleResume();
    }

    public void UnSuscribeMailbox(string mailbox) {
      IdlePause();

      string command = GetTag() + "UNSUBSCRIBE " + mailbox.QuoteString();
      SendCommandCheckOK(command);
      IdleResume();
    }

    internal override void CheckResultOK(string response) {
      if (!IsResultOK(response)) {
        response = response.Substring(response.IndexOf(" ")).Trim();
        throw new Exception(response);
      }
    }

    internal bool IsResultOK(string response) {
      response = response.Substring(response.IndexOf(" ")).Trim();
      return response.ToUpper().StartsWith("OK");
    }
  }
}
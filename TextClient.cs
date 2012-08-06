using System;
using System.IO;
using System.Net.Sockets;

namespace AE.Net.Mail {
  public abstract class TextClient : IDisposable {
    protected TcpClient _Connection;
    protected Stream _Stream;

    private readonly object _ReadLock = new object();
    private readonly object _WriteLock = new object();
    private readonly object _RWLock = new object();

    public string Host { get; private set; }
    public int Port { get; set; }
    public bool Ssl { get; set; }
    public bool IsConnected { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool IsDisposed { get; private set; }
    public System.Text.Encoding Encoding { get; set; }

    public TextClient() {
      Encoding = System.Text.Encoding.GetEncoding(1252);
    }

    internal abstract void OnLogin(string username, string password);
    internal abstract void OnLogout();
    internal abstract void CheckResultOK(string result);

    protected virtual void OnConnected(string result) {
      CheckResultOK(result);
    }

    protected virtual void OnDispose() { }

    public void Login(string username, string password) {
      if (!IsConnected) {
        throw new Exception("You must connect first!");
      }
      IsAuthenticated = false;
      OnLogin(username, password);
      IsAuthenticated = true;
    }

    public void Logout() {
      IsAuthenticated = false;
      OnLogout();
    }


    public void Connect(string hostname, int port, bool ssl, bool skipSslValidation) {
      System.Net.Security.RemoteCertificateValidationCallback validateCertificate = null;
      if (skipSslValidation)
        validateCertificate = (sender, cert, chain, err) => true;
      Connect(hostname, port, ssl, validateCertificate);
    }

    public void Connect(string hostname, int port, bool ssl, System.Net.Security.RemoteCertificateValidationCallback validateCertificate) {
      try {
        Host = hostname;
        Port = port;
        Ssl = ssl;

        _Connection = new TcpClient(hostname, port);
        _Stream = _Connection.GetStream();
        if (ssl) {
          System.Net.Security.SslStream sslStream;
          if (validateCertificate != null)
            sslStream = new System.Net.Security.SslStream(_Stream, false, validateCertificate);
          else
            sslStream = new System.Net.Security.SslStream(_Stream, false);
          _Stream = sslStream;
          sslStream.AuthenticateAsClient(hostname);
        }

        OnConnected(GetResponse());

        IsConnected = true;
        Host = hostname;
      } catch (Exception) {
        IsConnected = false;
        Utilities.TryDispose(ref _Stream);
        throw;
      }
    }

    protected void CheckConnectionStatus() {
      if (IsDisposed)
        throw new ObjectDisposedException(this.GetType().Name);
      if (!IsConnected)
        throw new Exception("You must connect first!");
      if (!IsAuthenticated)
        throw new Exception("You must authenticate first!");
    }

    protected virtual void SendCommand(string command) {
      var bytes = System.Text.Encoding.Default.GetBytes(command + "\r\n");

      lock (_WriteLock) {
        _Stream.Write(bytes, 0, bytes.Length);
      };
    }

    protected string SendCommandGetResponse(string command) {
      string response = null;
      lock (_RWLock) {
        SendCommand(command);
        response = GetResponse();
      }
      return response;
    }

    protected virtual string GetResponse() {
      lock (_ReadLock) {
        byte b;
        using (var mem = new System.IO.MemoryStream()) {
          while (true) {
            b = (byte)_Stream.ReadByte();
            if ((b == 10 || b == 13)) {
              if (mem.Length > 0 && b == 10) {
                return Encoding.GetString(mem.ToArray());
              }
            } else {
              mem.WriteByte(b);
            }
          }
        }
      }
    }

    protected void SendCommandCheckOK(string command) {
      CheckResultOK(SendCommandGetResponse(command));
    }

    public void Disconnect() {
      if (IsAuthenticated)
        Logout();

      Utilities.TryDispose(ref _Stream);
      Utilities.TryDispose(ref _Connection);
    }

    /// <summary>
    /// Releases all resources used by this ImapClient object.
    /// </summary>
    public void Dispose() {
      if (IsDisposed) return;
      lock (this) {
        if (IsDisposed) return;
        IsDisposed = true;
        Disconnect();

        try {
          OnDispose();
        } catch (Exception) { }

        _Stream = null;
        _Connection = null;
      }
      GC.SuppressFinalize(this);
    }
  }
}

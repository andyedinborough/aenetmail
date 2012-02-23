using System;
using System.IO;
using System.Net.Sockets;

namespace AE.Net.Mail {
  public abstract class TextClient : IDisposable {
    protected TcpClient _Connection;
    protected Stream _Stream;
    protected StreamReader _Reader;
    //protected StreamWriter _Writer;

    public string Host { get; private set; }

    public int Port { get; set; }
    public bool Ssl { get; set; }
    public bool IsConnected { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public bool IsDisposed { get; private set; }

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


    public void Connect(string hostname, int port, bool ssl) {
      try {
        Host = hostname;
        Port = port;
        Ssl = ssl;

        _Connection = new TcpClient(hostname, port);
        _Stream = _Connection.GetStream();
        if (ssl) {
          var sslStream = new System.Net.Security.SslStream(_Stream, false);
          _Stream = sslStream;
          sslStream.AuthenticateAsClient(hostname);
        }

        _Reader = new StreamReader(_Stream);
        //_Writer = new StreamWriter(_Stream);
        string info = _Reader.ReadLine();
        OnConnected(info);

        IsConnected = true;
        Host = hostname;
      } catch (Exception) {
        IsConnected = false;
        Utilities.TryDispose(ref _Reader);
        //Utilities.TryDispose(ref _Writer);
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
      _Stream.Write(bytes, 0, bytes.Length);
    }

    protected string SendCommandGetResponse(string command) {
      SendCommand(command);
      return GetResponse();
    }

    protected virtual string GetResponse() {
      return _Reader.ReadLine();
    }

    protected void SendCommandCheckOK(string command) {
      CheckResultOK(SendCommandGetResponse(command));
    }

    public void Disconnect() {
      Logout();

      Utilities.TryDispose(ref _Reader);
      //Utilities.TryDispose(ref _Writer);
      Utilities.TryDispose(ref _Stream);
      Utilities.TryDispose(ref _Connection);
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing) {
      if (disposing) {
        Disconnect();

        try {
          OnDispose();
        } catch (Exception) { }

        IsDisposed = true;
        _Stream = null;
        _Reader = null;
        _Connection = null;
      }
    }
  }
}

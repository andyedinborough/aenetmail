using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace AE.Net.Mail {
  public abstract class TextClient : IDisposable {
    protected TcpClient _Connection;
    protected Stream _Stream;
    protected BlockingCollection<String> _Responses = new BlockingCollection<String>();
    private Thread _ReadThread;

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
          var sslSream = new System.Net.Security.SslStream(_Stream);
          _Stream = sslSream;
          sslSream.AuthenticateAsClient(hostname);
        }

        //Create a new thread to retrieve data (needed for Imap Idle).
        _ReadThread = new Thread(ReceiveData);
        _ReadThread.Name = "_ReadThread";
        _ReadThread.Start();

        string info = _Responses.Take();
        OnConnected(info);

        IsConnected = true;
        Host = hostname;
      } catch (Exception) {
        IsConnected = false;
        throw;
      }
    }

    private void ReceiveData() {
      try {
        string line;
        using (var reader = new StreamReader(_Stream, System.Text.Encoding.Default))
          while (true) {
            line = reader.ReadLine();
            if (!_Responses.IsAddingCompleted) {
              _Responses.Add(line);
            }
          }
      } catch (Exception) { } //we don't want thread exceptions killing us
    }

    protected void CheckConnectionStatus() {
      if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name);
      if (!IsConnected) throw new Exception("You must connect first!");
      if (!IsAuthenticated) throw new Exception("You must authenticate first!");
    }

    protected virtual void SendCommand(string command) {
      byte[] data = System.Text.Encoding.Default.GetBytes(command + "\r\n");
      _Stream.Write(data, 0, data.Length);
    }

    protected string SendCommandGetResponse(string command) {
      SendCommand(command);
      return GetResponse();
    }

    protected virtual string GetResponse() {
      return _Responses.Take();
    }

    protected virtual bool TryGetResponse(out string result, int milliseconds = 200) {
      return _Responses.TryTake(out result, milliseconds);
    }

    protected void SendCommandCheckOK(string command) {
      CheckResultOK(SendCommandGetResponse(command));
    }

    public void Disconnect() {
      Logout();
      _Responses.CompleteAdding();
      if (_Stream != null) {
        _Stream.Dispose();
        _Stream = null;
      }
      if (_ReadThread != null && !_ReadThread.Join(2000)) {
        _ReadThread.Abort();
      }
    }

    public void Dispose() {
      Disconnect();

      try {
        OnDispose();
      } catch (Exception) { }


      IsDisposed = true;
      _Stream = null;
      _ReadThread = null;
      _Connection = null;
    }
  }
}

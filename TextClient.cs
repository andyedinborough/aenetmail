using System;
using System.IO;
using Org.Mentalis.Security.Ssl;

namespace AE.Net.Mail {
    public abstract class TextClient : IDisposable {
        protected SecureTcpClient _Connection;
        protected SecureNetworkStream _Stream;
        protected StreamReader _Reader;

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

                var protocol = ssl ? SecureProtocol.Tls1 | SecureProtocol.Ssl3 : SecureProtocol.None;
                SecurityOptions options = new SecurityOptions(protocol);
                options.Certificate = null;
                options.Entity = ConnectionEnd.Client;
                options.CommonName = hostname;
                options.VerificationType = CredentialVerification.Auto;
                options.Flags = SecurityFlags.Default;
                options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;

                //_Connection = new TcpClient(hostname, port);
                _Connection = new SecureTcpClient(hostname, port, options);
                _Stream = _Connection.GetStream();

                _Reader = new StreamReader(_Stream, System.Text.Encoding.Default);
                string info = _Reader.ReadLine();
                OnConnected(info);

                IsConnected = true;
                Host = hostname;
            } catch (Exception) {
                IsConnected = false;
                throw;
            }
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
            return _Reader.ReadLine();
        }

        protected void SendCommandCheckOK(string command) {
            CheckResultOK(SendCommandGetResponse(command));
        }

        public void Disconnect() {
            Logout();
            if (_Reader != null) {
                _Reader.Dispose();
            }
            if (_Stream != null) {
                _Stream.Dispose();
            }
        }

        public void Dispose() {
            try {
                OnDispose();
            } catch (Exception) { }

            Disconnect();

            IsDisposed = true;
            _Stream = null;
            _Reader = null;
            _Connection = null;
        }
    }
}

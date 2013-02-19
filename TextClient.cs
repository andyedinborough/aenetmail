using System;
using System.IO;
#if WINDOWS_PHONE
using SocketEx;
using NetSecurity = Portable.Utils.Security;
#else
using System.Net.Sockets;
using NetSecurity = System.Net.Security;
#endif

namespace AE.Net.Mail {
	public abstract class TextClient : IDisposable {
		protected TcpClient _Connection;
		protected Stream _Stream;

		public virtual string Host { get; private set; }
		public virtual int Port { get; set; }
		public virtual bool Ssl { get; set; }
		public virtual bool IsConnected { get; private set; }
		public virtual bool IsAuthenticated { get; private set; }
		public virtual bool IsDisposed { get; private set; }
		public virtual System.Text.Encoding Encoding { get; set; }

		public TextClient() {
#if WINDOWS_PHONE
            Encoding = System.Text.Encoding.GetEncoding("windows-1252");
#else
            Encoding = System.Text.Encoding.GetEncoding(1252);
#endif
		}

		internal abstract void OnLogin(string username, string password);
		internal abstract void OnLogout();
		internal abstract void CheckResultOK(string result);

		protected virtual void OnConnected(string result) {
			CheckResultOK(result);
		}

		protected virtual void OnDispose() { }

		public virtual void Login(string username, string password) {
			if (!IsConnected) {
				throw new Exception("You must connect first!");
			}
			IsAuthenticated = false;
			OnLogin(username, password);
			IsAuthenticated = true;
		}

		public virtual void Logout() {
			IsAuthenticated = false;
			OnLogout();
		}


		public virtual void Connect(string hostname, int port, bool ssl, bool skipSslValidation) {
			NetSecurity.RemoteCertificateValidationCallback validateCertificate = null;
			if (skipSslValidation)
#if WINDOWS_PHONE
                validateCertificate = (sender, err) => true;
#else
                validateCertificate = (sender, cert, chain, err) => true;
#endif
			Connect(hostname, port, ssl, validateCertificate);
		}

		public virtual void Connect(string hostname, int port, bool ssl, NetSecurity.RemoteCertificateValidationCallback validateCertificate) {
			try {
				Host = hostname;
				Port = port;
				Ssl = ssl;

				_Connection = new TcpClient(hostname, port);
				_Stream = _Connection.GetStream();
				if (ssl) {
					NetSecurity.SslStream sslStream;
					if (validateCertificate != null)
						sslStream = new NetSecurity.SslStream(_Stream, false, validateCertificate);
					else
						sslStream = new NetSecurity.SslStream(_Stream, false);
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

		protected virtual void CheckConnectionStatus() {
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

		protected virtual string SendCommandGetResponse(string command) {
			SendCommand(command);
			return GetResponse();
		}

		protected virtual string GetResponse() {
			int max = 0;
			return _Stream.ReadLine(ref max, Encoding, null);
		}

		protected virtual void SendCommandCheckOK(string command) {
			CheckResultOK(SendCommandGetResponse(command));
		}

		public virtual void Disconnect() {
			if (IsAuthenticated)
				Logout();

			Utilities.TryDispose(ref _Stream);
			Utilities.TryDispose(ref _Connection);
		}

		public virtual void Dispose() {
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

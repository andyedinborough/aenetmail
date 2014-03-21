using System;
using System.IO;
using System.Net.Sockets;

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

		public event EventHandler<WarningEventArgs> Warning;

		protected virtual void RaiseWarning(MailMessage mailMessage, string message) {
			var warning = Warning;
			if (warning != null) {
				warning(this, new WarningEventArgs { MailMessage = mailMessage, Message = message });
			}
		}

		public TextClient() {
			Encoding = System.Text.Encoding.GetEncoding(1252);
		}

		internal abstract void OnLogin(string username, string password);
		internal abstract void OnLogout();
		internal abstract void CheckResultOK(string result);

		protected virtual void OnConnected(string result) {
			CheckResultOK(result);
		}

		public virtual void Login(string username, string password) {
			if (!IsConnected) {
				throw new Exception("You must connect first!");
			}
			IsAuthenticated = false;
			OnLogin(username, password);
			IsAuthenticated = true;
		}

		public virtual void Logout() {
			OnLogout();
			IsAuthenticated = false;
		}

		public virtual void Connect(string hostname, int port, bool ssl, bool skipSslValidation) {
			System.Net.Security.RemoteCertificateValidationCallback validateCertificate = null;
			if (skipSslValidation)
				validateCertificate = (sender, cert, chain, err) => true;
			Connect(hostname, port, ssl, validateCertificate);
		}

		public virtual void Connect(string hostname, int port, bool ssl, System.Net.Security.RemoteCertificateValidationCallback validateCertificate) {
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

		protected virtual string GetResponse(int Timeout = 10000) {
			int max = 0;
			return _Stream.ReadLine(ref max, Encoding, null, Timeout);
		}

		protected virtual void SendCommandCheckOK(string command) {
			CheckResultOK(SendCommandGetResponse(command));
		}

		public virtual void Disconnect() {
			if (!IsConnected)
				return;
			if (IsAuthenticated) {
				Logout();
			}
			IsConnected = false;
			Utilities.TryDispose(ref _Stream);
			Utilities.TryDispose(ref _Connection);
		}

		~TextClient() {
			Dispose(false);
		}
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing) {
			if (!IsDisposed && disposing)
				lock (this)
					if (!IsDisposed && disposing) {
						IsDisposed = true;
						Disconnect();
					}

			_Stream = null;
			_Connection = null;
		}
	}
}

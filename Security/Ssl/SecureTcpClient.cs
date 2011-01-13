/*
 *   Mentalis.org Security Library
 * 
 *     Copyright © 2002-2005, The Mentalis.org Team
 *     All rights reserved.
 *     http://www.mentalis.org/
 *
 *
 *   Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions
 *   are met:
 *
 *     - Redistributions of source code must retain the above copyright
 *        notice, this list of conditions and the following disclaimer. 
 *
 *     - Neither the name of the Mentalis.org Team, nor the names of its contributors
 *        may be used to endorse or promote products derived from this
 *        software without specific prior written permission. 
 *
 *   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 *   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 *   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
 *   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 *   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 *   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 *   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 *   OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Net;
using System.Net.Sockets;

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Provides secure client connections for TCP network services.
	/// </summary>
	public class SecureTcpClient {
		/// <summary>
		/// Initializes a new instance of the <see cref="SecureTcpClient"/> class.
		/// </summary>
		/// <remarks>
		///  The default constructor initializes a new SecureTcpClient. You must call the Connect method to establish a remote host connection.
		/// </remarks>
		public SecureTcpClient() : this(new SecurityOptions(SecureProtocol.None)) {}
		/// <summary>
		/// Initializes a new instance of <see cref="SecureTcpClient"/> bound to the specified local endpoint.
		/// </summary>
		/// <param name="localEP">The IPEndPoint to which you bind the TCP Socket.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localEP"/> is null (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
		public SecureTcpClient(IPEndPoint localEP) : this(localEP, new SecurityOptions(SecureProtocol.None)) {}
		/// <summary>
		/// Initializes a new instance of the <see cref="SecureTcpClient"/> class and connects to the specified port on the specified host.
		/// </summary>
		/// <param name="hostname">DNS name of the remote host to which you intend to connect.</param>
		/// <param name="port">Port number of the remote host to which you intend to connect.</param>
		/// <exception cref="ArgumentNullException"><paramref name="hostname"/> is null (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than MinPort -or- <paramref name="port"/> is greater than MaxPort.</exception>
		/// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostname"/><br>-or-</br><br>an error occurred while connecting to the remote host.</br></exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public SecureTcpClient(string hostname, int port) : this(hostname, port, new SecurityOptions(SecureProtocol.None)) {}
		/// <summary>
		/// Initializes a new instance of the <see cref="SecureTcpClient"/> class.
		/// </summary>
		/// <param name="options">The security options to use.</param>
		public SecureTcpClient(SecurityOptions options) {
			m_Client = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);
		}
		/// <summary>
		/// Initializes a new instance of <see cref="SecureTcpClient"/> bound to the specified local endpoint.
		/// </summary>
		/// <param name="localEP">The IPEndPoint to which you bind the TCP Socket.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localEP"/> is null (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
		public SecureTcpClient(IPEndPoint localEP, SecurityOptions options) : this(options) {
			m_Client.Bind(localEP);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="SecureTcpClient"/> class and connects to the specified port on the specified host.
		/// </summary>
		/// <param name="hostname">DNS name of the remote host to which you intend to connect.</param>
		/// <param name="port">Port number of the remote host to which you intend to connect.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="hostname"/> is null (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than MinPort -or- <paramref name="port"/> is greater than MaxPort.</exception>
		/// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostname"/> -or- an error occurred while connecting to the remote host.</exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public SecureTcpClient(string hostname, int port, SecurityOptions options) : this(options) {
			if (hostname == null)
				throw new ArgumentNullException();
			Connect(hostname, port);
		}
		/// <summary>
		/// Initializes a new instance of <see cref="SecureTcpClient"/>.
		/// </summary>
		/// <param name="socket">The accepted socket.</param>
		/// <remarks>This constructor is used by the SecureTcpListener class.</remarks>
		internal SecureTcpClient(SecureSocket socket) : base() {
			m_Client = socket;
			m_Active = true;
		}
        /// <summary> 
        /// Create a new SecureTcpClient based on an existing one.
        /// </summary>
        /// <param name="client">The SecureTcpClient to copy from.</param>
        public SecureTcpClient(SecureTcpClient client) : base() {
            m_Client = client.Client;
            m_Active = client.Active;
            m_CleanedUp = client.CleanedUp;
            m_DataStream = client.DataStream;
        } 
		/// <summary>
		/// Connects the client to a remote TCP host using the specified remote network endpoint.
		/// </summary>
		/// <param name="remoteEP">The IP endpoint to which you intend to connect.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="remoteEP"/> parameter is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="SecureSocket"/> has been closed.</exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public virtual void Connect(IPEndPoint remoteEP) {
			Client.Connect(remoteEP);
			Active = true;
		}
		/// <summary>
		/// Connects the client to a remote TCP host using the specified IP address and port number.
		/// </summary>
		/// <param name="address">The IP address of the host to which you intend to connect.</param>
		/// <param name="port">The port number to which you intend to connect.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="address"/> parameter is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than MinPort -or- <paramref name="port"/> is greater than MaxPort.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="SecureSocket"/> has been closed.</exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public virtual void Connect(IPAddress address, int port) {
			if (address == null)
				throw new ArgumentNullException();
			Connect(new IPEndPoint(address, port));
		}
		/// <summary>
		/// Connects the client to the specified port on the specified host.
		/// </summary>
		/// <param name="hostname">The DNS name of the remote host to which you intend to connect.</param>
		/// <param name="port">The port number of the remote host to which you intend to connect.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="hostname"/> parameter is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than MinPort -or- <paramref name="port"/> is greater than MaxPort.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="SecureSocket"/> has been closed.</exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public virtual void Connect(string hostname, int port) {
			if (hostname == null)
				throw new ArgumentNullException();
			Connect(Dns.GetHostEntry(hostname).AddressList[0], port);
		}
		/// <summary>
		/// Returns the stream used to send and receive data.
		/// </summary>
		/// <returns>The underlying <see cref="SecureNetworkStream"/>.</returns>
		/// <exception cref="ObjectDisposedException">The <see cref="SecureTcpClient"/> has been closed.</exception>
		/// <exception cref="InvalidOperationException">The SecureTcpClient is not connected to a remote host.</exception>
		public virtual SecureNetworkStream GetStream() {
			if (CleanedUp)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (!Client.Connected)
				throw new InvalidOperationException();
			if (DataStream == null) {
				DataStream = new SecureNetworkStream(Client, false);
			}
			return DataStream;
		}
		/// <summary>
		/// Closes the TCP connection.
		/// </summary>
		/// <exception cref="SocketException">An error occurs while closing the Socket.</exception>
		public void Close() {
			Dispose();
		}
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SecureTcpClient"/> and optionally releases the managed resources.
		/// </summary>
		protected virtual void Dispose() {
			if (!CleanedUp) {
				CleanedUp = true;
				Active = false;
				if (DataStream != null) {
					DataStream.Close();
					DataStream = null;
				}
				if (Client.Connected) {
					try {
						Client.Shutdown(SocketShutdown.Both);
					} catch {}
				}
				Client.Close();
			}
		}
		/// <summary>
		/// Gets or sets information about the sockets linger time.
		/// </summary>
		/// <value>A LingerOption.</value>
		/// <remarks>This property controls the length of time that the underlying Socket will remain open after a call to Close, when data remains to be sent. If the Enabled property of the LingerOption is true, then data will continue to be sent to the network with a time out of LingerOption.LingerTime seconds. Once the data is sent, or if the time-out expires, the connection is closed and any unsent data is lost. If the Enabled property of the LingerOption is false , then the connection will close, even if data remains to be sent.</remarks>
		public LingerOption LingerState {
			get {
				return (LingerOption)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
			}
		}
		/// <summary>
		/// Gets or sets a value that enables a delay when send or receive buffers are not full.
		/// </summary>
		/// <value><b>true</b> to disable a delay, otherwise <b>false</b>.</value>
		/// <remarks>When NoDelay is false, TCP does not send a packet over the network until it has collected a significant amount of outgoing data. Because of the amount of overhead in a TCP segment, sending small amounts of data would be very inefficient. However, situations do exist where you might want to send very small amounts of data or expect immediate responses from each packet you send. Your decision should weigh the relative importance of network efficiency versus application requirements.</remarks>
		public bool NoDelay {
			get {
				return !((int)Client.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay) == 0);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}
		/// <summary>
		/// Gets or sets the size of the receive buffer.
		/// </summary>
		/// <value>The size of the receive buffer, in bytes.</value>
		/// <remarks>The ReceiveBufferSize property gets or sets the number of bytes that you are expecting to store in the receive buffer for each read operation.</remarks>
		public int ReceiveBufferSize {
			get {
				return (int)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
			}
		}
		/// <summary>
		/// Gets or sets the amount of time a <see cref="SecureTcpClient"/> will wait to receive data once initiated.
		/// </summary>
		/// <value>The time-out value of the connection in milliseconds.</value>
		/// <remarks>The ReceiveTimeout property determines the amount of time a SecureTcpClient will wait to receive data after a read is initiated. This time is measured in milliseconds. The underlying Socket will throw a SocketException if a read is initiated, and the ReceiveTimeout expires.</remarks>
		public int ReceiveTimeout {		
			get {
				return (int)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
			}
		}
		/// <summary>
		/// Gets or sets the size of the send buffer.
		/// </summary>
		/// <value>The size of the send buffer, in bytes.</value>
		/// <remarks>The SendBufferSize property gets or sets the number of bytes to store in the send buffer for each send operation.</remarks>
		public int SendBufferSize {
			get {
				return (int)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
			}
		}
		/// <summary>
		/// Gets or sets the amount of time a SecureTcpClient will wait to receive confirmation after you initiate a send.
		/// </summary>
		/// <value>The send time-out value, in milliseconds.</value>
		/// <remarks>After you initiate a send, the underlying <see cref="SecureSocket"/> returns the number of bytes actually sent to the host. The SendTimeout property determines the amount of time a TcpClient will wait before receiving the number of bytes returned by the SecureSocket class. The underlying SecureSocket will throw a SocketException if a send is initiated and the SendTimeout expires.</remarks>
		public int SendTimeout {
			get {
				return (int)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
			}
			set{
				Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
			}
		}
		/// <summary>
		/// Gets or sets the underlying <see cref="SecureSocket"/>.
		/// </summary>
		/// <value>The underlying Network Socket.</value>
		/// <remarks>SecureTcpClient creates a SecureSocket to send and receive data over a network. Classes deriving from SecureTcpClient can use this property to get or set this SecureSocket. Use the underlying SecureSocket returned from Client, if you require access beyond that which SecureTcpClient provides. You can also use Client to set the underlying SecureSocket to an existing SecureSocket. This might be useful if you want to take advantage of the simplicity of SecureTcpClient using a pre-existing SecureSocket.</remarks>
		protected SecureSocket Client {
			get {
				return m_Client;
			}
			set {
				m_Client = value;
			}
		}
		/// <summary>
		/// Gets or set a value that indicates whether a connection has been made.
		/// </summary>
		/// <value><b>true</b> if the connection has been made; otherwise, <b>false</b>.</value>
		/// <remarks>Classes deriving from SecureTcpClient can use this property to keep track of the underlying <see cref="SecureSocket"/> connection state.</remarks>
		protected bool Active {
			get {
				return m_Active;
			}
			set {
				m_Active = value;
			}
		}
		/// <summary>
		/// Gets or sets a value that indicates whether the underlying SecureSocket has been closed or not.
		/// </summary>
		/// <value><b>true</b> if the underlying <see cref="SecureSocket"/> has been closed, <b>false</b> otherwise.</value>
		protected bool CleanedUp{
			get {
				return m_CleanedUp;
			}
			set {
				m_CleanedUp = value;
			}
		}
		/// <summary>
		/// Gets or sets the underlying <see cref="SecureNetworkStream"/> associated with this SecureTcpClient.
		/// </summary>
		/// <value>An instance of the SecureNetworkStream class.</value>
		protected SecureNetworkStream DataStream {
			get {
				return m_DataStream;
			}
			set {
				m_DataStream = value;
			}
		}
		/// <summary>Holds the value of the <see cref="Active"/> property.</summary>
		private bool m_Active = false;
		/// <summary>Holds the value of the <see cref="CleanedUp"/> property.</summary>
		private bool m_CleanedUp = false;
		/// <summary>Holds the value of the <see cref="Client"/> property.</summary>
		private SecureSocket m_Client;
		/// <summary>Holds the value of the <see cref="DataStream"/> property.</summary>
		private SecureNetworkStream m_DataStream = null;
	}
}
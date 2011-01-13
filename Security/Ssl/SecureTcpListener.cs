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
	/// Listens for secure connections from TCP network clients.
	/// </summary>
	public class SecureTcpListener {
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class that listens on the specified port.
		/// </summary>
		/// <param name="port">The port on which to listen. If this number is 0, the system will assign an open port.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not between MinPort and MaxPort.</exception>
		/// <remarks><paramref name="port"/> specifies the local port number on which you intend to listen. When you call Start, SecureTcpListener uses the default network interface to listen for connections on the specified port.</remarks>
		public SecureTcpListener(int port) : this(IPAddress.Any, port) {}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class that listens on the specified port.
		/// </summary>
		/// <param name="port">The port on which to listen. If this number is 0, the system will assign an open port.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentOutOfRangeException">The port parameter is not between MinPort and MaxPort.</exception>
		/// <remarks><paramref name="port"/> specifies the local port number on which you intend to listen. When you call Start, SecureTcpListener uses the default network interface to listen for connections on the specified port.</remarks>
		public SecureTcpListener(int port, SecurityOptions options) : this(IPAddress.Any, port, options) {}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class that listens to the specified IP address and port.
		/// </summary>
		/// <param name="localaddr">The local IP address.</param>
		/// <param name="port">The port on which to listen.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localaddr"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not between MinPort and MaxPort.</exception>
		public SecureTcpListener(IPAddress localaddr, int port) : this(new IPEndPoint(localaddr, port)) {}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class that listens to the specified IP address and port.
		/// </summary>
		/// <param name="localaddr">The local IP address.</param>
		/// <param name="port">The port on which to listen.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localaddr"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not between MinPort and MaxPort.</exception>
		public SecureTcpListener(IPAddress localaddr, int port, SecurityOptions options) : this(new IPEndPoint(localaddr, port), options) {}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class with the specified local endpoint.
		/// </summary>
		/// <param name="localEP">The local endpoint to which to bind the listener Socket.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <remarks><paramref name="localEP"/> specifies the local <see cref="IPEndPoint"/>. This constructor creates an underlying SecureSocket, and binds that SecureSocket to <paramref name="localEP"/>. If you call the Start method, TcpListener will listen for connections on <paramref name="localEP"/>.</remarks>
		public SecureTcpListener(IPEndPoint localEP) : this(localEP, new SecurityOptions(SecureProtocol.None, null, ConnectionEnd.Server)) {}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class with the specified local endpoint.
		/// </summary>
		/// <param name="localEP">The local endpoint to which to bind the listener Socket.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <remarks><paramref name="localEP"/> specifies the local <see cref="IPEndPoint"/>. This constructor creates an underlying SecureSocket, and binds that SecureSocket to <paramref name="localEP"/>. If you call the Start method, TcpListener will listen for connections on <paramref name="localEP"/>.</remarks>
		public SecureTcpListener(IPEndPoint localEP, SecurityOptions options) {
			if (localEP == null)
				throw new ArgumentNullException();
			m_LocalEndpoint = localEP;
			m_SecurityOptions = options;
		}
		/// <summary>
		/// Initializes a new instance of the SecureTcpListener class with the specified listener SecureSocket.
		/// </summary>
		/// <param name="listener">The listener <see cref="SecureSocket"/>.</param>
		/// <param name="options">The security options to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="listener"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An error occurs while reading the LocalEndPoint property.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		protected SecureTcpListener(SecureSocket listener, SecurityOptions options) {
			if (listener == null)
				throw new ArgumentNullException();
			m_Server = listener;
			m_LocalEndpoint = listener.LocalEndPoint;
			m_SecurityOptions = options;
		}
		/// <summary>
		/// Gets a value that indicates whether SecureTcpListener is actively listening for client connections.
		/// </summary>
		/// <value><b>true</b> if SecureTcpListener is actively listening; otherwise <b>false</b>.</value>
		/// <remarks>Classes deriving from SecureTcpListener can use this property to keep track of the underlying <see cref="SecureSocket"/> connection state.</remarks>
		protected bool Active {
			get {
				return (m_Server != null);
			}
		}
		/// <summary>
		/// Gets the underlying <see cref="EndPoint"/> of the current SecureTcpListener.
		/// </summary>
		/// <value>An instance of the EndPoint class used to bind the underlying <see cref="SecureSocket"/>.</value>
		/// <remarks>You can use LocalEndpoint if you want to identify the local network interface and port number being used to listen for incoming client connection requests.<br><b>Note</b>   To obtain address and port information, you must explicitly cast LocalEndpoint to return an <see cref="IPEndPoint"/>. You can then use the various methods within IPEndPoint to retrieve the desired information.</br></remarks>
		public EndPoint LocalEndpoint {
			get {
				if (Server == null)
					return m_LocalEndpoint;
				else
					return Server.LocalEndPoint;
			}
		}
		/// <summary>
		/// Gets the underlying <see cref="SecureSocket"/>.
		/// </summary>
		/// <value>An instance of the SecureSocket class that provides the underlying network socket.</value>
		/// <remarks>SecureTcpListener creates a SecureSocket to listen for incoming client connection requests. Classes deriving from SecureTcpListener can use this property to get this Socket. Use the underlying SecureSocket returned by the Server property if you require access beyond that which SecureTcpListener provides.<br><b>Note</b>    Server only returns the SecureSocket used to listen for incoming client connection requests. Use the AcceptSocket method to accept a pending connection request and obtain a SecureSocket for sending and receiving data. You can also use the AcceptTcpClient method to accept a pending connection request and obtain a SecureTcpClient for sending and receiving data.</br></remarks>
		protected SecureSocket Server {
			get {
				return m_Server;
			}
		}
		/// <summary>
		/// Gets the security options that are used for incoming connections.
		/// </summary>
		/// <value>A <see cref="SecurityOptions"/> instance.</value>
		protected SecurityOptions SecurityOptions {
			get {
				return m_SecurityOptions;
			}
		}
		/// <summary>
		/// Accepts a pending connection request.
		/// </summary>
		/// <returns>A <see cref="SecureSocket"/> used to send and receive data.</returns>
		/// <exception cref="InvalidOperationException">The listener has not been started with a call to Start.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="SecurityException">Unable to create the SSPI credentials.</exception>
		/// <remarks>AcceptSocket returns a SecureSocket that you can use to send and receive data. This SecureSocket is initialized with the IP address and port number of the remote machine. You can use any of the Send and Receive methods available in the Socket class to communicate with the remote machine.<br><b>Note</b>   When you finish using the Socket, be sure to call its Close method.</br><br><b>Note</b>   If your application is relatively simple, consider using the AcceptTcpClient method rather than AcceptSocket. SecureTcpClient provides you with simple methods for sending and receiving data over a network.</br></remarks>
		public virtual SecureSocket AcceptSocket() {
			if (Server == null)
				throw new InvalidOperationException();
			return (SecureSocket)Server.Accept();
		}
		/// <summary>
		/// Accepts a pending connection request.
		/// </summary>
		/// <returns>A SecureTcpClient used to send and receive data.</returns>
		/// <exception cref="InvalidOperationException">The listener has not been started with a call to Start.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="SecurityException">Unable to create the SSPI credentials.</exception>
		/// <remarks>
		/// AcceptTcpClient returns a SecureTcpClient that you can use to send and receive data. Use SecureTcpClient.GetStream to obtain the underlying SecureNetworkStream of the SecureTcpClient. SecureNetworkStream inherits from Stream, which provides a rich collection of methods and properties for network communications.
		/// <br><b>Note</b>   When you are through with the returned SecureTcpClient, be sure to call it's Close method.</br>
		/// <br><b>Note</b>   If you want greater flexibility than a SecureTcpClient offers, consider using AcceptSocket.</br>
		/// </remarks>
		public virtual SecureTcpClient AcceptTcpClient() {
			if (Server == null)
				throw new InvalidOperationException();
			return new SecureTcpClient(AcceptSocket());
		}
		/// <summary>
		/// Determines if there are pending connection requests.
		/// </summary>
		/// <returns><b>true</b> if connections are pending; otherwise, <b>false</b>.</returns>
		/// <exception cref="InvalidOperationException">The listener has not been started with a call to Start.</exception>
		/// <remarks>Pending polls for the underlying <see cref="SecureSocket"/> to determine if there are pending connections.</remarks>
		public virtual bool Pending() {
			if (Server == null)
				throw new InvalidOperationException();
			return Server.Poll(0, SelectMode.SelectRead);
		}
		/// <summary>
		/// Starts listening to network requests.
		/// </summary>
		/// <exception cref="SocketException">An error occurs while opening the network socket.</exception>
		/// <exception cref="SecurityException">Unable to create the SSPI credentials.</exception>
		public virtual void Start() {
			if (Server != null)
				return;
			EndPoint ep = LocalEndpoint;
			m_Server = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, this.SecurityOptions);
			Server.Bind(ep);
			Server.Listen(int.MaxValue);
		}
		/// <summary>
		/// Closes the listener.
		/// </summary>
		/// <remarks>Stop closes the listener.</remarks>
		public virtual void Stop() {
			if (Server == null)
				return;
			Server.Close();
			m_Server = null;
		}
		/// <summary>Holds the value if the <see cref="LocalEndpoint"/> property.</summary>
		private EndPoint m_LocalEndpoint;
		/// <summary>Holds the value if the <see cref="Server"/> property.</summary>
		private SecureSocket m_Server;
		/// <summary>Holds the value if the <see cref="SecurityOptions"/> property.</summary>
		private SecurityOptions m_SecurityOptions;
	}
}
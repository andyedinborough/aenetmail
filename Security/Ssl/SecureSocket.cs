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
using System.Collections;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl.Shared;

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Implements the Berkeley sockets interface and optionally encrypts/decrypts transmitted data.
	/// </summary>
	/// <remarks>Any public static (Shared in Visual Basic) members of this type are safe for multithreaded operations. Any instance members are not guaranteed to be thread safe.</remarks>
	public class SecureSocket : VirtualSocket {
		/// <summary>
		/// Initializes a new instance of the SecureSocket class.
		/// </summary>
		/// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
		/// <param name="socketType">One of the <see cref="SocketType"/> values.</param>
		/// <param name="protocolType">One of the <see cref="ProtocolType"/> values.</param>
		/// <exception cref="SocketException">The combination of addressFamily, socketType, and protocolType results in an invalid socket.</exception>
		/// <remarks>The SecureSocket will act like a normal Socket and will not use a secure transfer protocol.</remarks>
		public SecureSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : this(addressFamily, socketType, protocolType, new SecurityOptions(SecureProtocol.None)) {}
		/// <summary>
		/// Initializes a new instance of the SecureSocket class.
		/// </summary>
		/// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
		/// <param name="socketType">One of the <see cref="SocketType"/> values.</param>
		/// <param name="protocolType">One of the <see cref="ProtocolType"/> values.</param>
		/// <param name="options">The <see cref="SecurityOptions"/> to use.</param>
		/// <exception cref="SecurityException">An error occurs while changing the security protocol.</exception>
		public SecureSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, SecurityOptions options) : base(addressFamily, socketType, protocolType) {
			m_SentShutdownNotification = false;
			ChangeSecurityProtocol(options);
		}
		/// <summary>
		/// Initializes a new instance of the SecureSocket class.
		/// </summary>
		/// <param name="accepted">The accepted <see cref="Socket"/> instance.</param>
		/// <param name="options">The <see cref="SecurityOptions"/> to use.</param>
		/// <exception cref="SecurityException">An error occurs while changing the security protocol.</exception>
		internal SecureSocket(Socket accepted, SecurityOptions options) : base(accepted) {
			m_SentShutdownNotification = false;
			ChangeSecurityProtocol(options);
		}
		/// <summary>
		/// Changes the security protocol. This method can only be used to 'upgrade' a connection from no-security to either SSL or TLS.
		/// </summary>
		/// <param name="options">The new <see cref="SecurityOptions"/> parameters.</param>
		/// <exception cref="SecurityException">An error occurs while changing the security protocol.</exception>
		/// <remarks>
		/// Programs should only call this method if there is no active <see cref="Connect"/>, <see cref="Accept"/>, <see cref="Send"/> or <see cref="Receive"/>!
		/// </remarks>
		public void ChangeSecurityProtocol(SecurityOptions options) {
			if (options == null)
				throw new ArgumentNullException();
			if (m_Options != null && m_Options.Protocol != SecureProtocol.None)
				throw new ArgumentException("Only changing from a normal connection to a secure connection is supported.");
			if (base.ProtocolType != ProtocolType.Tcp && options.Protocol != SecureProtocol.None)
				throw new SecurityException("Security protocols require underlying TCP connections!");
			// check SecurityOptions structure
			if (options.Protocol != SecureProtocol.None) {
				if (options.Entity == ConnectionEnd.Server && options.Certificate == null)
					throw new ArgumentException("The certificate cannot be set to a null reference when creating a server socket.");
				if (options.Certificate != null && !options.Certificate.HasPrivateKey())
					throw new ArgumentException("If a certificate is specified, it must have a private key.");
				if (((int)options.AllowedAlgorithms & (int)SslAlgorithms.NULL_COMPRESSION) == 0)
					throw new ArgumentException("The allowed algorithms field must contain at least one compression algorithm.");
				if (((int)options.AllowedAlgorithms ^ (int)SslAlgorithms.NULL_COMPRESSION) == 0)
					throw new ArgumentException("The allowed algorithms field must contain at least one cipher suite.");
				if (options.VerificationType == CredentialVerification.Manual && options.Verifier == null)
					throw new ArgumentException("A CertVerifyEventHandler is required when using manual certificate verification.");
			}
			m_Options = (SecurityOptions)options.Clone();
			if (options.Protocol != SecureProtocol.None) {
				if (this.Connected)
					m_Controller = new SocketController(this, base.InternalSocket, options);
			}
		}
		/// <summary>
		/// Establishes a connection to a remote device and optionally negotiates a secure transport protocol.
		/// </summary>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote device.</param>
		/// <exception cref="ArgumentNullException">The remoteEP parameter is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the <see cref="SecureSocket"/>.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">The security negotiation failed.</exception>
		public override void Connect(EndPoint remoteEP) {
			if (SecureProtocol == SecureProtocol.None) {
				base.Connect(remoteEP);
			} else {
				this.EndConnect(this.BeginConnect(remoteEP, null, null));
			}
		}
		/// <summary>
		/// Begins an asynchronous request for a connection to a network device.
		/// </summary>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote device.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous connection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="remoteEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while creating the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		public override IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state) {
			if (SecureProtocol == SecureProtocol.None)
				return base.BeginConnect(remoteEP, callback, state);
			// secure BeginConnect
			if (remoteEP == null)
				throw new ArgumentNullException();
			if (m_ConnectResult != null)
				throw new SocketException(); // BeginConnect already called
			AsyncResult ret = new AsyncResult(callback, state, null);
			m_ConnectResult = ret;
			base.BeginConnect(remoteEP, new AsyncCallback(OnConnect), null);
			return ret;
		}
		/// <summary>
		/// Called then the <see cref="SecureSocket"/> connects to the remote host.
		/// </summary>
		/// <param name="ar">An <see cref="IAsyncResult"/> instance.</param>
		private void OnConnect(IAsyncResult ar) {
			try {
				base.EndConnect(ar);
				m_Controller = new SocketController(this, base.InternalSocket, m_Options);
			} catch (Exception e) {
				m_ConnectResult.AsyncException = e;
			}
			m_ConnectResult.Notify();
		}
		/// <summary>
		/// Ends a pending asynchronous connection request.
		/// </summary>
		/// <param name="asyncResult">The result of the asynchronous operation.</param>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginConnect"/> method.</exception>
		/// <exception cref="InvalidOperationException"><see cref="EndConnect"/> was previously called for the asynchronous connection.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while negotiating the security protocol.</exception>
		public override void EndConnect(IAsyncResult asyncResult) {
			if (SecureProtocol == SecureProtocol.None) {
				base.EndConnect(asyncResult);
				return;
			}
			// Make sure everything is in order
			if (asyncResult == null)
				throw new ArgumentNullException();
			if (m_ConnectResult == null)
				throw new InvalidOperationException();
			if (asyncResult != m_ConnectResult)
				throw new ArgumentException();
			// Process the (secure) EndConnect
			// block if the operation hasn't ended yet
			AsyncResult ar = m_ConnectResult;
            while(!ar.IsCompleted) {
                ar.AsyncWaitHandle.WaitOne(200, false);
            }
			m_ConnectResult = null;
			if (ar.AsyncException != null)
				throw ar.AsyncException;
		}
		/// <summary>
		/// Creates a new <see cref="SecureSocket"/> to handle an incoming connection request.
		/// </summary>
		/// <returns>A SecureSocket to handle an incoming connection request.</returns>
		/// <remarks>The returned <see cref="VirtualSocket"/> can be cast to a SecureSocket if necessary.</remarks>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to create the credentials.</exception>
		public override VirtualSocket Accept() {
			return EndAccept(BeginAccept(null, null));
		}
		/// <summary>
		/// Begins an asynchronous request to create a new <see cref="SecureSocket"/> to accept an incoming connection request.
		/// </summary>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous SecureSocket creation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="callback"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while creating the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		public override IAsyncResult BeginAccept(AsyncCallback callback, object state) {
			if (m_AcceptResult != null)
				throw new SocketException();
			AsyncAcceptResult ret = new AsyncAcceptResult(callback, state, null);
			m_AcceptResult = ret;
			base.BeginAccept(new AsyncCallback(this.OnAccept), null);
			return ret;
		}
		private void OnAccept(IAsyncResult ar) {
			try {
				m_AcceptResult.AcceptedSocket = new SecureSocket(base.InternalEndAccept(ar), m_Options);
			} catch (Exception e) {
				m_AcceptResult.AsyncException = e;
			}
			m_AcceptResult.Notify();
		}
		/// <summary>
		/// Ends an asynchronous request to create a new <see cref="SecureSocket"/> to accept an incoming connection request.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>A SecureSocket to handle the incoming connection.</returns>
		/// <remarks>The returned <see cref="VirtualSocket"/> can be cast to a SecureSocket if necessary.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not created by a call to <see cref="BeginAccept"/>.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to create the credentials -or- client authentication error.</exception>
		public override VirtualSocket EndAccept(IAsyncResult asyncResult) {
			// Make sure everything is in order
			if (asyncResult == null)
				throw new ArgumentNullException();
			if (m_AcceptResult == null)
				throw new InvalidOperationException();
			if (m_AcceptResult != asyncResult)
				throw new ArgumentException();
			AsyncAcceptResult ar = m_AcceptResult;
			// Process the (secure) EndAccept
			// block if the operation hasn't ended yet
            while (!ar.IsCompleted) {
                ar.AsyncWaitHandle.WaitOne(200, false);
            }
			m_AcceptResult = null;
			if (ar.AsyncException != null)
				throw ar.AsyncException;
			return ar.AcceptedSocket;
		}
		/// <summary>
		/// Sends data to a connected <see cref="SecureSocket"/>, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <returns>The number of bytes sent to the SecureSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified size is zero.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to encrypt the data.</exception>
		public override int Send(byte[] buffer) {
			if (buffer == null) 
				throw new ArgumentNullException();
			return this.Send(buffer, 0, buffer.Length, SocketFlags.None);
		}
		/// <summary>
		/// Sends data to a connected <see cref="SecureSocket"/>, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <returns>The number of bytes sent to the SecureSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified size is zero.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to encrypt the data.</exception>
		public override int Send(byte[] buffer, SocketFlags socketFlags) {
			if (buffer == null)
				throw new ArgumentNullException();
			return this.Send(buffer, 0, buffer.Length, socketFlags);
		}
		/// <summary>
		/// Sends data to a connected <see cref="SecureSocket"/>, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <returns>The number of bytes sent to the SecureSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size parameter exceeds the size of buffer.</exception>
		/// <exception cref="ArgumentException">The specified size is zero.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to encrypt the data.</exception>
		public override int Send(byte[] buffer, int size, SocketFlags socketFlags) {
			return this.Send(buffer, 0, size, socketFlags);
		}
		/// <summary>
		/// Sends data to a connected <see cref="SecureSocket"/>, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="offset">The position in the data buffer to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <returns>The number of bytes sent to the SecureSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The offset or size parameter exceeds the size of buffer.</exception>
		/// <exception cref="ArgumentException">The specified size is zero.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">Unable to encrypt the data.</exception>
		public override int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
			if (SecureProtocol == SecureProtocol.None)
				return base.Send(buffer, offset, size, socketFlags);
			else
				return this.EndSend(this.BeginSend(buffer, offset, size, socketFlags, null, null));
		}
		/// <summary>
		/// Sends data asynchronously to a connected <see cref="SecureSocket"/>.
		/// </summary>
		/// <param name="buffer">The data to send.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="ArgumentException">The specified size is zero.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while encrypting the data.</exception>
		public override IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) {
			if (SecureProtocol == SecureProtocol.None)
				return base.BeginSend(buffer, offset, size, socketFlags, callback, state);
			if (!Connected)
				throw new SocketException();
			if (buffer == null)
				throw new ArgumentNullException();
			if (size == 0)
				throw new ArgumentException();
			if (offset < 0 || offset >= buffer.Length || size > buffer.Length - offset || size < 0)
				throw new ArgumentOutOfRangeException();
			// begin secure send
			return m_Controller.BeginSend(buffer, offset, size, callback, state);
		}
		/// <summary>
		/// Ends a pending asynchronous send.
		/// </summary>
		/// <param name="asyncResult">The result of the asynchronous operation.</param>
		/// <returns>If successful, the number of bytes sent to the SecureSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginSend"/> method.</exception>
		/// <exception cref="InvalidOperationException"><see cref="EndSend"/> was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurs while communicating with the remote host.</exception>
		public override int EndSend(IAsyncResult asyncResult) {
			if (SecureProtocol == SecureProtocol.None)
				return base.EndSend(asyncResult);
			if (asyncResult == null)
				throw new ArgumentNullException();
			TransferItem ti = m_Controller.EndSend(asyncResult);
			if (ti == null)
				throw new ArgumentException();
            while (!ti.AsyncResult.IsCompleted) {
                ti.AsyncResult.AsyncWaitHandle.WaitOne(200, false);
            }
			if (ti.AsyncResult.AsyncException != null)
				throw new SecurityException("An error occurs while communicating with the remote host.", ti.AsyncResult.AsyncException);
			return ti.OriginalSize;
		}
		/// <summary>
		/// Receives data from a connected <see cref="SecureSocket"/> into a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while decrypting the received data.</exception>
		public override int Receive(byte[] buffer) {
			if (buffer == null)
				throw new ArgumentNullException();
			return this.Receive(buffer, 0, buffer.Length, SocketFlags.None);
		}
		/// <summary>
		/// Receives data from a connected <see cref="SecureSocket"/> into a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while decrypting the received data.</exception>
		public override int Receive(byte[] buffer, SocketFlags socketFlags) {
			if (buffer == null)
				throw new ArgumentNullException();
			return this.Receive(buffer, 0, buffer.Length, socketFlags);
		}
		/// <summary>
		/// Receives data from a connected <see cref="SecureSocket"/> into a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SecureSocket"/> values.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while decrypting the received data.</exception>
		public override int Receive(byte[] buffer, int size, SocketFlags socketFlags) {
			return this.Receive(buffer, 0, size, socketFlags);
		}
		/// <summary>
		/// Receives data from a connected <see cref="SecureSocket"/> into a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="offset">The location in buffer to store the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SecurityException">An error occurred while decrypting the received data.</exception>
		public override int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
			if (SecureProtocol == SecureProtocol.None) {
				return base.Receive(buffer, offset, size, socketFlags);
			} else {
				return this.EndReceive(this.BeginReceive(buffer, offset, size, socketFlags, null, null));
			}
		}
		/// <summary>
		/// Begins to asynchronously receive data from a connected SecureSocket.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to store the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="ObjectDisposedException">SecureSocket has been closed.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The offset parameter is outside the bounds of buffer or size is either smaller or larger than the buffer size.</exception>
		public override IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) {
			if (SecureProtocol == SecureProtocol.None)
				return base.BeginReceive(buffer, offset, size, socketFlags, callback, state);
			if (!Connected && m_SentShutdownNotification)
				throw new SocketException();
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || (offset >= buffer.Length && size != 0) || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException();
			return m_Controller.BeginReceive(buffer, offset, size, callback, state);
		}
		/// <summary>
		/// Ends a pending asynchronous read.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginReceive"/> method.</exception>
		/// <exception cref="InvalidOperationException"><see cref="EndReceive"/> was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The <see cref="SecureSocket"/> has been closed.</exception>
		/// <exception cref="SecurityException">An error occurs while communicating with the remote host.</exception>
		public override int EndReceive(IAsyncResult asyncResult) {
			if (SecureProtocol == SecureProtocol.None)
				return base.EndReceive(asyncResult);
			// Make sure everything is in order
			if (asyncResult == null)
				throw new ArgumentNullException();
			TransferItem ti = m_Controller.EndReceive(asyncResult);
			if (ti == null)
				throw new ArgumentException();
			// Process the (secure) EndReceive
			// block if the operation hasn't ended yet
            while (!ti.AsyncResult.IsCompleted) {
                ti.AsyncResult.AsyncWaitHandle.WaitOne(200, false);
            }
			if (ti.AsyncResult.AsyncException != null)
				throw new SecurityException("An error occurs while communicating with the remote host.\r\n" + ti.AsyncResult.AsyncException.ToString(), ti.AsyncResult.AsyncException);
			if (ti.Transferred == 0)
				m_SentShutdownNotification = true;
			return ti.Transferred;
		}
		/// <summary>
		/// Shuts down the secure connection.
		/// </summary>
		/// <exception cref="ObjectDisposedException">SecureSocket has been closed.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="SecurityException">An error occurs while shutting the secure connection down.</exception>
		public override void Shutdown(SocketShutdown how) {
			this.EndShutdown(this.BeginShutdown(null, null));
		}
		/// <summary>
		/// Begins an asynchronous request to shut the connection down.
		/// </summary>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous shutdown.</returns>
		/// <exception cref="InvalidOperationException"><see cref="BeginShutdown"/> has already been called.</exception>
		// Thanks to Michael J. Moore and Stefan Bernbo for notifying us about a bug in this method.
		public IAsyncResult BeginShutdown(AsyncCallback callback, object state) {
			if (m_ShutdownResult != null)
				throw new InvalidOperationException();
			AsyncResult ar = new AsyncResult(callback, state, null);
			m_ShutdownResult = ar;
			if (!this.Connected) {
				ar.Notify(null);
			} else if (SecureProtocol == SecureProtocol.None) {
				base.Shutdown(SocketShutdown.Both);
				ar.Notify(null);
			} else {
				m_Controller.BeginShutdown(new AsyncCallback(this.OnShutdown), null);
			}
			return ar;
		}
		/// <summary>
		/// Called when the shutdown data has been sent to the remote server.
		/// </summary>
		/// <param name="ar">An <see cref="IAsyncResult"/> instance.</param>
		private void OnShutdown(IAsyncResult ar) {
			try {
				m_Controller.EndShutdown(ar);
			} catch {
				// eat exceptions; we don't throw them in Socket.EndShutdown [not really important]
				// m_ShutdownResult.AsyncException = e;
			}
			m_ShutdownResult.Notify();
		}
		/// <summary>
		/// Ends an asynchronous request to shut the connection down.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> that references the asynchronous shutdown.</param>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="InvalidOperationException"><see cref="BeginShutdown"/> has not been called first.</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> has not been returned by a call to <see cref="BeginShutdown"/>.</exception>
		public void EndShutdown(IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException();
			if (m_ShutdownResult == null)
				throw new InvalidOperationException();
			if (asyncResult != m_ShutdownResult)
				throw new ArgumentException();
			// Process the EndSecureShutdown
			// block if the operation hasn't ended yet
			AsyncResult ar = m_ShutdownResult;
			while (!ar.IsCompleted) {
				ar.AsyncWaitHandle.WaitOne(200, false);
			}
			m_ShutdownResult = null;
			//if (ar.AsyncException != null)  // eat exceptions; they're not really important
			//	throw ar.AsyncException;
		}
		/// <summary>
		/// Gets the amount of data that has been received from the network and is available to be read.
		/// </summary>
		/// <value>The number of bytes of data that has been received from the network and are available to be read.</value>
		/// <exception cref="ObjectDisposedException">The SecureSocket has been closed.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <exception cref="SecurityException">An error occurs while interpreting the security data.</exception>
		public override int Available {
			get {
				if (SecureProtocol == SecureProtocol.None)
					return base.Available;
				if (m_IsDisposed)
					throw new ObjectDisposedException(this.GetType().FullName);
//				if (!Connected)		// closed socket doesn't mean there are no bytes in the decrypted buffer
//					throw new SocketException();
				return m_Controller.Available;
			}
		}
		/// <summary>
		/// Queues a renegotiation request.
		/// </summary>
		/// <exception cref="SocketException">An operating system error occurs while accessing the SecureSocket.</exception>
		/// <remarks>
		/// Use the QueueRenegotiate function with caution. Some SSL/TLS clients or server do not support renegotiation.
		/// For instance, requesting a renegotiation in the middle of sending a HTTP request to a MS IIS server causes the connection to be shut down.
		/// Renegotiations should only be used when a small private key [512 bits] is used and the connection is active for a long period of time.
		/// </remarks>
		public void QueueRenegotiate() {
			if (!Connected)
				throw new SocketException();
			m_Controller.QueueRenegotiate();
		}
		/// <summary>
		/// Forces a SecureSocket connection to close.
		/// </summary>
		public override void Close() {
			base.Close();
			if (!m_IsDisposed) {
				if (m_Controller != null)
					m_Controller.Dispose();
				m_IsDisposed = true;
			}
		}
		/// <summary>
		/// Frees resources used by the <see cref="SecureSocket"/> class.
		/// </summary>
		/// <remarks>
		/// The SecureSocket class finalizer calls the Close method to close the SecureSocket and free resources associated with the SecureSocket.
		/// </remarks>
		~SecureSocket() {
			Close();
		}
		/// <summary>
		/// Gets the local certificate.
		/// </summary>
		/// <value>An instance of the <see cref="Certificate"/> class.</value>
		public Certificate LocalCertificate {
			get {
				return m_Options.Certificate;
			}
		}
		/// <summary>
		/// Gets the remote certificate.
		/// </summary>
		/// <value>An instance of the <see cref="Certificate"/> class -or- a null reference (<b>Nothing</b> in Visual Basic) if no certificate has been received.</value>
		public Certificate RemoteCertificate {
			get {
				if (m_Controller == null)
					return null;
				return m_Controller.RemoteCertificate;
			}
		}
		/// <summary>
		/// Gets the security protocol in use.
		/// </summary>
		/// <value>A bitwise combination of the <see cref="SecureProtocol"/> values.</value>
		public SecureProtocol SecureProtocol {
			get {
				return m_Options.Protocol;
			}
		}
		/// <summary>
		/// Gets the credential type.
		/// </summary>
		/// <value>One of the <see cref="ConnectionEnd"/> values.</value>
		public ConnectionEnd Entity {
			get {
				return m_Options.Entity;
			}
		}
		/// <summary>
		/// Gets the common name of the remote host.
		/// </summary>
		/// <value>A string representing the common name of the remote host.</value>
		/// <remarks>
		/// The common name of the remote host is usually the domain name.
		/// </remarks>
		public string CommonName {
			get {
				return m_Options.CommonName;
			}
		}
		/// <summary>
		/// Gets the credential verification type.
		/// </summary>
		/// <value>One of the <see cref="CredentialVerification"/> values.</value>
		public CredentialVerification VerificationType {
			get {
				return m_Options.VerificationType;
			}
		}
		/// <summary>
		/// Gets the verify delegate.
		/// </summary>
		/// <value>A <see cref="CertVerifyEventHandler"/> instance.</value>
		public CertVerifyEventHandler Verifier {
			get {
				return m_Options.Verifier;
			}
		}
		/// <summary>
		/// Gets the security flags of the connection.
		/// </summary>
		/// <value>A bitwise combination of the <see cref="SecurityFlags"/> values.</value>
		public SecurityFlags SecurityFlags {
			get {
				return m_Options.Flags;
			}
		}
		/// <summary>
		/// Gets the active encryption cipher suite.
		/// </summary>
		/// <value>One of the <see cref="SslAlgorithms"/> values.</value>
		/// <remarks>
		/// This value is properly initialized after the handshake of the SSL or TLS protocol. Currently, there's no way of knowing when a handshake is completed. However as soon as either a Send or a Receive returns, the handshake must be complete.
		/// <p>If SSL or TLS is not used, this property returns <b>SslAlgorithms.NONE</b>.</p>
		/// </remarks>
		public SslAlgorithms ActiveEncryption {
			get {
				if (m_Controller == null)
					return SslAlgorithms.NONE;
				return m_Controller.ActiveEncryption;
			}
		}
		private SocketController m_Controller;
		private SecurityOptions m_Options;
		private AsyncAcceptResult m_AcceptResult;
		private AsyncResult m_ConnectResult;
		private AsyncResult m_ShutdownResult;
		private bool m_SentShutdownNotification;
		private bool m_IsDisposed;
		/// <summary>
		///	Gets or sets a value that indicates whether the VirtualSocket is in blocking mode.
		/// </summary>
		/// <value><b>true</b> if the VirtualSocket will block; otherwise, <b>false</b>. The default is <b>true</b>.</value>
		/// <remarks>This property is not supported for SSL/TLS sockets. It can only be used if the SecureProtocol is set to None. Asynchronous behavior in SSL or TLS mode can be achieved by calling the asynchronous methods.</remarks>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <exception cref="NotSupportedException">Non-blocking sockets are not supported in SSL or TLS mode.</exception>
		public override bool Blocking {
			get {
				return base.Blocking;
			}
			set {
				if (!value && SecureProtocol != SecureProtocol.None)
					throw new NotSupportedException("Non-blocking sockets are not supported in SSL or TLS mode. Use the asynchronous methods instead.");
				base.Blocking = value;
			}
		}
		/// <summary>
		/// Determines the status of the VirtualSocket.
		/// </summary>
		/// <param name="microSeconds">The time to wait for a response, in microseconds.</param>
		/// <param name="mode">One of the <see cref="SelectMode"/> values.</param>
		/// <returns>See the Socket documentation for the return values.</returns>
		/// <remarks>This property is not supported for SSL/TLS sockets. It can only be used if the SecureProtocol is set to None. Asynchronous behavior in SSL or TLS mode can be achieved by calling the asynchronous methods.</remarks>
		/// <exception cref="NotSupportedException">The mode parameter is not one of the SelectMode values -or- the socket is in SSL or TLS mode.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>Set microSeconds parameter to a negative integer if you would like to wait indefinitely for a response.</remarks>
		public override bool Poll(int microSeconds, SelectMode mode) {
			if (SecureProtocol != SecureProtocol.None)
				throw new NotSupportedException("The Poll method is not supported in SSL or TLS mode. Use the asynchronous methods and the Available property instead.");
			return base.Poll(microSeconds, mode);
		}
	}
}
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

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Implements the Berkeley sockets interface.
	/// </summary>
	/// <remarks>
	/// The VirtualSocket class implements exactly the same methods as the System.Net.Sockets.Socket class,
	/// however all these methods are marked as virtual so they can be overridden in derived classes.
	/// In addition to the constructor specified by the Socket class, the VirtualSocket class also
	/// has a constructor that accepts an already created Socket; this can be very useful is you have
	/// to override the Accept and BeginAccept/EndAccept methods in a derived class.
	/// </remarks>
	public class VirtualSocket {
		/// <summary>
		/// Initializes a new instance of the VirtualSocket class.
		/// </summary>
		/// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
		/// <param name="socketType">One of the <see cref="SocketType"/> values.</param>
		/// <param name="protocolType">One of the <see cref="ProtocolType"/> values.</param>
		/// <exception cref="SocketException">The combination of <paramref name="addressFamily"/>, <paramref name="socketType"/>, and <paramref name="protocolType"/> results in an invalid socket.</exception>
		/// <remarks>The <paramref name="addressFamily"/> parameter specifies the addressing scheme that the VirtualSocket uses, the <paramref name="socketType"/> parameter specifies the type of the VirtualSocket, and <paramref name="protocolType"/> specifies the protocol used by the VirtualSocket. The three parameters are not independent. Some address families restrict which protocols can be used with them, and often the socket type is implicit in the protocol. If the combination of address family, socket type, and protocol type results in an invalid VirtualSocket, a SocketException is thrown.<br>The AddressFamily enumeration defines the valid address families, the SocketType enumeration defines the valid socket types, and the ProtocolType enumeration defines the valid protocol types.</br></remarks>
		public VirtualSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
			m_InternalSocket = new Socket(addressFamily, socketType, protocolType);
		}
		/// <summary>
		/// Initializes a new instance of the VirtualSocket class.
		/// </summary>
		/// <param name="internalSocket">The accepted socket.</param>
		/// <exception cref="ArgumentNullException"><paramref name="internalSocket"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		protected VirtualSocket(Socket internalSocket) {
			if (internalSocket == null)
				throw new ArgumentNullException();
			m_InternalSocket = internalSocket;
		}
		/// <summary>
		///	Gets or sets the internal <see cref="Socket"/> value.
		/// </summary>
		/// <value>An instance of the Socket class.</value>
		protected Socket InternalSocket {
			get {
				return m_InternalSocket;
			}
			set {
				m_InternalSocket = value;
			}
		}
		/// <summary>
		///	Gets or sets a value that indicates whether the VirtualSocket is in blocking mode.
		/// </summary>
		/// <value><b>true</b> if the VirtualSocket will block; otherwise, <b>false</b>. The default is <b>true</b>.</value>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		public virtual bool Blocking {
			get {
				return InternalSocket.Blocking;
			}
			set {
				InternalSocket.Blocking = value;
			}
		}
		/// <summary>
		///	Gets the address family of the VirtualSocket.
		/// </summary>
		/// <value>One of the <see cref="AddressFamily"/> values.</value>
		/// <remarks>AddressFamily specifies the addressing scheme that an instance of the VirtualSocket class can use. This property is read-only and is set when the VirtualSocket is created.</remarks>
		public virtual AddressFamily AddressFamily {
			get {
				return InternalSocket.AddressFamily;
			}
		}
		/// <summary>
		///	Gets the amount of data that has been received from the network and is available to be read.
		/// </summary>
		/// <value>The number of bytes of data that has been received from the network and are available to be read.</value>
		/// <remarks>If you are using a Stream VirtualSocket type, the available data is generally the total amount of data queued on the current instance. If you are using a message-oriented VirtualSocket type such as Dgram, the available data is the first message in the input queue.</remarks>
		/// <exception cref="SocketException">An error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		public virtual int Available {
			get {
				return InternalSocket.Available;
			}
		}
		/// <summary>
		///	Gets a value indicating whether a VirtualSocket is connected to a remote resource.
		/// </summary>
		/// <value><b>true</b> if the VirtualSocket is connected to a remote resource; otherwise, <b>false</b>.</value>
		/// <remarks>Gets the connection state of the VirtualSocket. This property will return the latest known state of the VirtualSocket. When it returns <b>false</b>, the VirtualSocket was either never connected, or no longer connected. When it returns <b>true</b>, the VirtualSocket was connected at the time of the last I/O operation.<br><b>Note</b>   There is no guarantee that the VirtualSocket is still Connected even though Connected returns <b>true</b>.</br></remarks>
		public virtual bool Connected {
			get {
				return InternalSocket.Connected;
			}
		}
		/// <summary>
		///	Gets the operating system handle for the VirtualSocket.
		/// </summary>
		/// <value>An <see cref="IntPtr"/> representing the operating system handle for the VirtualSocket.</value>
		public virtual IntPtr Handle {
			get {
				return InternalSocket.Handle;
			}
		}
		/// <summary>
		///	Gets the local endpoint.
		/// </summary>
		/// <value>The local endpoint that the VirtualSocket is using for communications.</value>
		/// <remarks>The LocalEndPoint property contains the network connection information associated with the local network device. LocalEndPoint is set by calling the Bind method.</remarks>
		/// <exception cref="SocketException">An error occurs while reading the property.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		public virtual EndPoint LocalEndPoint {
			get {
				return InternalSocket.LocalEndPoint;
			}
		}
		/// <summary>
		///	Gets the protocol type of the VirtualSocket.
		/// </summary>
		/// <value>One of the <see cref="ProtocolType"/> values.</value>
		/// <remarks>ProtocolType is set when the VirtualSocket is created, and specifies the protocol used by that VirtualSocket.</remarks>
		public virtual ProtocolType ProtocolType {
			get {
				return InternalSocket.ProtocolType;
			}
		}
		/// <summary>
		///	Gets the remote endpoint.
		/// </summary>
		/// <value>The remote endpoint that the VirtualSocket is using for communications.</value>
		/// <remarks>The RemoteEndPoint property gets the network connection information associated with the remote host. RemoteEndPoint is set by VirtualSocket methods that establish a connection to a remote host.</remarks>
		/// <exception cref="SocketException">An error occurs while reading the property.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		public virtual EndPoint RemoteEndPoint {
			get {
				return InternalSocket.RemoteEndPoint;
			}
		}
		/// <summary>
		///	Gets the type of the VirtualSocket.
		/// </summary>
		/// <value>One of the <see cref="SocketType"/> values.</value>
		/// <remarks>SocketType is set when the class is created.</remarks>
		public virtual SocketType SocketType {
			get {
				return InternalSocket.SocketType;
			}
		}
		/// <summary>
		/// Creates a new VirtualSocket to handle an incoming connection request.
		/// </summary>
		/// <returns>A VirtualSocket to handle an incoming connection request.</returns>
		/// <exception cref="SocketException">The VirtualSocket is invalid.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The Accept method extracts the first connection request from the queue of pending requests and creates a new VirtualSocket to handle it.</remarks>
		public virtual VirtualSocket Accept() {
			return new VirtualSocket(InternalAccept());
		}
		/// <summary>
		/// Creates a new Socket to handle an incoming connection request.
		/// </summary>
		/// <returns>A Socket to handle an incoming connection request.</returns>
		/// <exception cref="SocketException">The VirtualSocket is invalid.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The InternalAccept method extracts the first connection request from the queue of pending requests and creates a new <see cref="Socket"/> to handle it.</remarks>
		protected virtual Socket InternalAccept() {
			return InternalSocket.Accept();
		}
		/// <summary>
		/// Begins an asynchronous request to create a new VirtualSocket to accept an incoming connection request.
		/// </summary>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request. </param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous VirtualSocket creation.</returns>
		/// <exception cref="SocketException">An operating system error occurs while creating the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The BeginAccept method starts an asynchronous request to create a VirtualSocket to handle an incoming connection request. You must create a callback method that implements the AsyncCallback delegate. This callback method should use the <see cref="EndAccept"/> method to retrieve the VirtualSocket.</remarks>
		public virtual IAsyncResult BeginAccept(AsyncCallback callback, object state) {
			return InternalSocket.BeginAccept(callback, state);
		}
		/// <summary>
		/// Begins an asynchronous request for a connection to a network device.
		/// </summary>
		/// <param name="remoteEP">An EndPoint that represents the remote device.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains state information for this request. </param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous connection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="remoteEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while creating the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The BeginConnect method starts an asynchronous request for a remote host connection. You must create a callback method that implements the AsyncCallback delegate. This callback method should use the <see cref="EndConnect"/> method to return the VirtualSocket.</remarks>
		public virtual IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state) {
			return InternalSocket.BeginConnect(remoteEP, callback, state);
		}
		/// <summary>
		/// Begins to asynchronously receive data from a connected VirtualSocket.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to store the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">VirtualSocket has been closed.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is outside the bounds of buffer or size is either smaller or larger than the buffer size.</exception>
		/// <remarks>The BeginReceive method starts asynchronously reading data from a VirtualSocket. You must create a callback method that implements the AsyncCallback delegate. This callback method should use the <see cref="EndReceive"/> method to return the data read from the VirtualSocket.</remarks>
		public virtual IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) {
			return InternalSocket.BeginReceive(buffer, offset, size, socketFlags, callback, state);
		}
		/// <summary>
		/// Begins to asynchronously receive data from a specified network device.
		/// </summary>
		/// <param name="buffer">The storage location for the received data.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to store the data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the source of the data.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
		/// <exception cref="ArgumentException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br>
		/// <br><paramref name="remoteEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</br><br>-or-</br><br><paramref name="offset"/> is outside the bounds of buffer.</br></exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		public virtual IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state) {
			return InternalSocket.BeginReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
		}
		/// <summary>
		/// Sends data asynchronously to a connected VirtualSocket.
		/// </summary>
		/// <param name="buffer">The data to send.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The BeginSend method starts asynchronously sending data through a socket. You must create a callback method that implements the AsyncCallback delegate. This callback method should use the <see cref="EndSend"/> method to complete sending data.</remarks>
		public virtual IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) {
			return InternalSocket.BeginSend(buffer, offset, size, socketFlags, callback, state);
		}
		/// <summary>
		/// Sends data asynchronously to a specific remote host.
		/// </summary>
		/// <param name="buffer">The data to send. </param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
		/// <param name="size">The number of bytes to send. </param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote device. </param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate. </param>
		/// <param name="state">An object containing state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send.</returns>
		/// <exception cref="ArgumentException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br><br><paramref name="remoteEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</br></exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The BeginSendTo method starts asynchronously sending data through a socket. You must create a callback method that implements the AsyncCallback delegate. This callback method should use the <see cref="EndSendTo"/> method to complete sending data.</remarks>
		public virtual IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state) {
			return InternalSocket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, callback, state);
		}
		/// <summary>
		/// Associates a VirtualSocket with a local endpoint.
		/// </summary>
		/// <param name="localEP">The local <see cref="EndPoint"/> to associate with the VirtualSocket.</param>
		/// <exception cref="ArgumentNullException"><paramref name="localEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>If you want to use a specific local endpoint, you can call the Bind method before you call the <see cref="Listen"/> or <see cref="Connect"/> methods.</remarks>
		public virtual void Bind(EndPoint localEP) {
			InternalSocket.Bind(localEP);
		}
		/// <summary>
		/// Forces a VirtualSocket connection to close.
		/// </summary>
		/// <remarks><p>The <see cref="Connected"/> property is set to <b>false</b> when the socket is closed.</p><p>The application should call <see cref="Shutdown"/> before calling Close to ensure that all pending data is sent or received before the VirtualSocket is closed.</p></remarks>
		public virtual void Close() {
			InternalSocket.Close();
		}
		/// <summary>
		/// Establishes a connection to a remote device.
		/// </summary>
		/// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote device.</param>
		/// <exception cref="ArgumentNullException"><paramref name="remoteEP"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The Connect method establishes a network connection between <see cref="LocalEndPoint"/> and the device identified by <paramref name="remoteEP"/>. Once the connection has been made, you can send data to the remote device with the <see cref="Send"/> method, or receive data from the remote device with the <see cref="Receive"/> method.</remarks>
		public virtual void Connect(EndPoint remoteEP) {
			InternalSocket.Connect(remoteEP);
		}
		/// <summary>
		/// Ends an asynchronous request to create a new VirtualSocket to accept an incoming connection request.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>A VirtualSocket to handle the incoming connection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not created by a call to <see cref="BeginAccept"/>.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The EndAccept method completes a request for a connection that was started with the BeginAccept method.</remarks>
		public virtual VirtualSocket EndAccept(IAsyncResult asyncResult) {
			return new VirtualSocket(InternalEndAccept(asyncResult));
		}
		/// <summary>
		/// Ends an asynchronous request to create a new <see cref="Socket"/> to accept an incoming connection request.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>A VirtualSocket to handle the incoming connection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not created by a call to <see cref="BeginAccept"/>.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The InternalEndAccept method completes a request for a connection that was started with the InternalBeginAccept method.</remarks>
		protected virtual Socket InternalEndAccept(IAsyncResult asyncResult) {
			return InternalSocket.EndAccept(asyncResult);
		}
		/// <summary>
		/// Ends a pending asynchronous connection request.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user-defined data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginConnect"/> method.</exception>
		/// <exception cref="InvalidOperationException">EndConnect was previously called for the asynchronous connection.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>To maintain the asynchronous nature of the operation, call this method from the callback delegate. You can pass either the <see cref="IAsyncResult"/> returned from BeginConnect or the callback delegate used as an input parameter to BeginConnect as the asyncresult parameter. The EndConnect method blocks.</remarks>
		public virtual void EndConnect(IAsyncResult asyncResult) {
			InternalSocket.EndConnect(asyncResult);
		}
		/// <summary>
		/// Ends a pending asynchronous read.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginReceive"/> method.</exception>
		/// <exception cref="InvalidOperationException">EndReceive was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>To maintain the asynchronous nature of the operation, call this method from the callback delegate. You can pass either the <see cref="IAsyncResult"/> returned from <see cref="BeginReceive"/> or the callback delegate used as an input parameter to BeginReceive as the asyncResult parameter. The EndReceive method blocks until the read ends.</remarks>
		public virtual int EndReceive(IAsyncResult asyncResult) {
			return InternalSocket.EndReceive(asyncResult);
		}
		/// <summary>
		/// Ends a pending asynchronous read from a specific endpoint.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <param name="endPoint">The source <see cref="EndPoint"/>.</param>
		/// <returns>If successful, the number of bytes received. If unsuccessful, returns 0 if the connection is closed by the remote host.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the BeginReceiveFrom method.</exception>
		/// <exception cref="InvalidOperationException">EndReceiveFrom was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>To maintain the asynchronous nature of the operation, call this method from the callback delegate. You can pass either the <see cref="IAsyncResult"/> returned from <see cref="BeginReceiveFrom"/> or the callback delegate used as an input parameter to BeginReceiveFrom. as the asyncResult parameter. The EndReceiveFrom method frees any resources allocated by the BeginReceiveFrom method. The EndReceiveFrom method blocks until read ends.</remarks>
		public virtual int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint) {
			return InternalSocket.EndReceiveFrom(asyncResult, ref endPoint);
		}
		/// <summary>
		/// Ends a pending asynchronous send.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data.</param>
		/// <returns>If successful, the number of bytes sent to the VirtualSocket; otherwise, an invalid VirtualSocket error.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginSend"/> method.</exception>
		/// <exception cref="InvalidOperationException">EndSend was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>To maintain the asynchronous nature of the operation, call this method from the callback delegate. You can pass either the <see cref="IAsyncResult"/> returned from <see cref="BeginSend"/> or the callback delegate used as an input parameter to BeginSend as the asyncResult parameter. The EndSend method frees any resources allocated by the BeginSend method. The EndSend method blocks until the send ends.<br>The EndSend method frees any resources allocated by the BeginSend method.</br></remarks>
		public virtual int EndSend(IAsyncResult asyncResult) {
			return InternalSocket.EndSend(asyncResult);
		}
		/// <summary>
		/// Ends a pending asynchronous send to a specific location.
		/// </summary>
		/// <param name="asyncResult">Stores state information for this asynchronous operation as well as any user defined data .</param>
		/// <returns>If successful, the number of bytes sent; otherwise, an invalid VirtualSocket error.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="BeginSendTo"/> method.</exception>
		/// <exception cref="InvalidOperationException">EndSendTo was previously called for the asynchronous read.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks><br>To maintain the asynchronous nature of the operation, call this method from the callback delegate. You can pass either the <see cref="IAsyncResult"/> returned from <see cref="BeginSendTo"/> or the callback delegate used as an input parameter to BeginSendTo as the asyncResult parameter. The EndSendTo method frees any resources allocated by the BeginSendTo method. The EndSendTo method blocks until send is complete.</br><br>The EndSendTo method frees any resources allocated by the BeginSendTo method.</br></remarks>
		public virtual int EndSendTo(IAsyncResult asyncResult) {
			return InternalSocket.EndSendTo(asyncResult);
		}
		/// <summary>
		/// This member overrides Object.GetHashCode.
		/// </summary>
		/// <returns>A hash code for the current VirtualSocket.</returns>
		public override int GetHashCode() {
			return InternalSocket.GetHashCode();
		}
		/// <summary>
		/// Gets the value of a specified socket option.
		/// </summary>
		/// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
		/// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
		/// <returns>The value of the option. When the optionName parameter is set to Linger the return value is an instance of the LingerOption. When optionName is set to AddMembership or DropMembership, the return value is an instance of the MulticastOption. When optionName is any other value, the return value is an integer.</returns>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>VirtualSocket options determine the behavior of the current instance. Upon successful completion, GetSocketOption returns an object describing the requested option. For example, if you specify Linger as the option, a LingerOption is returned.</remarks>
		public virtual object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName) {
			return InternalSocket.GetSocketOption(optionLevel, optionName);
		}
		/// <summary>
		/// Gets the specified VirtualSocket option setting.
		/// </summary>
		/// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
		/// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
		/// <param name="optionValue">The buffer that is to receive the option setting.</param>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>Socket options determine the behavior of the current Socket. Upon successful completion of this method, the array specified by the optionValue parameter contains the value of the specified Socket option. When the length of the optionValue array is smaller than the number of bytes required to store the value of the specified Socket option, a <see cref="SocketException"/> is thrown.</remarks>
		public virtual void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
			InternalSocket.GetSocketOption(optionLevel, optionName, optionValue);
		}
		/// <summary>
		/// Returns the value of the specified Socket option and returns in an array.
		/// </summary>
		/// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
		/// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
		/// <param name="optionLength">The length, in bytes, of the expected return value.</param>
		/// <returns>An array of bytes containing the value of the socket option.</returns>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The optionLength parameter sets the maximum size of the returned byte array. If the option value requires fewer bytes, the array will contain only that many bytes. If the option value requires more bytes, a SocketException will be thrown.</remarks>
		public virtual byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength) {
			return InternalSocket.GetSocketOption(optionLevel, optionName, optionLength);
		}
		/// <summary>
		/// Sets low-level operating modes for the VirtualSocket.
		/// </summary>
		/// <param name="ioControlCode">The control code of the operation to perform.</param>
		/// <param name="optionInValue">The input data required by the operation.</param>
		/// <param name="optionOutValue">The output data returned by the operation.</param>
		/// <returns>The number of bytes in optionOutValue parameter.</returns>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>The IOControl method provides low-level access to the operating system socket underlying the current instance of the VirtualSocket class. For more information about IOControl, see the WSAIoct documentation in MSDN.</remarks>
		public virtual int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue) {
			return InternalSocket.IOControl(ioControlCode, optionInValue, optionOutValue);
		}
		/// <summary>
		/// Places a VirtualSocket in a listening state.
		/// </summary>
		/// <param name="backlog">The Maximum length of the queue of pending connections.</param>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>In a listening state, the VirtualSocket will poll for incoming connection attempts. If you want to listen using a specific network interface on a specific port, you must call the Bind method first.</remarks>
		public virtual void Listen(int backlog) {
			InternalSocket.Listen(backlog);
		}
		/// <summary>
		/// Determines the status of the VirtualSocket.
		/// </summary>
		/// <param name="microSeconds">The time to wait for a response, in microseconds.</param>
		/// <param name="mode">One of the <see cref="SelectMode"/> values.</param>
		/// <returns>See the Socket documentation for the return values.</returns>
		/// <exception cref="NotSupportedException">The mode parameter is not one of the SelectMode values.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>Set microSeconds parameter to a negative integer if you would like to wait indefinitely for a response.</remarks>
		public virtual bool Poll(int microSeconds, SelectMode mode) {
			return InternalSocket.Poll(microSeconds, mode);
		}
		/// <summary>
		/// Receives data from a connected VirtualSocket in a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer. The offset defaults to 0, size defaults to the buffer length, and the socketFlags value defaults to None.</p>
		/// <p>The Blocking determines the behavior of this method when no incoming data is available. When false, a SocketException is thrown. When true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all the data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p><b>Note</b>   If you specify the OutOfBand flag as the socketFlags parameter, and the Socket is configured for in-line reception of out-of-band (OOB) data (using the OutOfBandInline option) and OOB data is available, then only OOB data is returned. When the Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but is not removed from the system buffer.</p>
		/// </remarks>
		public virtual int Receive(byte[] buffer) {
			return InternalSocket.Receive(buffer);
		}
		/// <summary>
		/// Receives data from a connected VirtualSocket in a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values. </param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer and the necessary SocketFlags. The offset defaults to 0, and the size defaults to the buffer length.</p>
		/// <p>The Blocking determines the behavior of this method when no incoming data is available. When false, a SocketException is thrown. When true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all the data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p><b>Note</b>   If you specify the OutOfBand flag as the socketFlags parameter, and the Socket is configured for in-line reception of out-of-band (OOB) data (using the OutOfBandInline option) and OOB data is available, then only OOB data is returned. When the Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but is not removed from the system buffer.</p>
		/// </remarks>
		public virtual int Receive(byte[] buffer, SocketFlags socketFlags) {
			return InternalSocket.Receive(buffer, socketFlags);
		}
		/// <summary>
		/// Receives data from a connected VirtualSocket in a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values. </param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer, the number of bytes you want to send, and the necessary SocketFlags. The offset defaults to 0.</p>
		/// <p>The Blocking determines the behavior of this method when no incoming data is available. When false, a SocketException is thrown. When true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all the data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p><b>Note</b>   If you specify the OutOfBand flag as the socketFlags parameter, and the Socket is configured for in-line reception of out-of-band (OOB) data (using the OutOfBandInline option) and OOB data is available, then only OOB data is returned. When the Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but is not removed from the system buffer.</p>
		/// </remarks>
		public virtual int Receive(byte[] buffer, int size, SocketFlags socketFlags) {
			return InternalSocket.Receive(buffer, size, socketFlags);
		}
		/// <summary>
		/// Receives data from a connected VirtualSocket in a specific location of the receive buffer.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="offset">The location in buffer to store the received data. </param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values. </param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>The Blocking determines the behavior of this method when no incoming data is available. When false, a SocketException is thrown. When true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all the data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p><b>Note</b>   If you specify the OutOfBand flag as the socketFlags parameter, and the Socket is configured for in-line reception of out-of-band (OOB) data (using the OutOfBandInline option) and OOB data is available, then only OOB data is returned. When the Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but is not removed from the system buffer.</p>
		/// </remarks>
		public virtual int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
			return InternalSocket.Receive(buffer, offset, size, socketFlags);
		}
		/// <summary>
		/// Receives a datagram in a specific location in the data buffer and stores the endpoint.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer, and EndPoint representing the remote host. The offset defaults to 0. The size defaults to the buffer length and the socketFlags value defaults to None.</p>
		/// <p>If you use a connectionless protocol, the remoteEP parameter contains the EndPoint associated with the Socket that sent the data. If you use a connection-oriented protocol, remoteEP is left unchanged. You must set the LocalEndPoint property before calling this method. When no incoming data is available, and the Blocking property is false, a SocketException is thrown. When Blocking is true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, then buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p>When the OutOfBand flag is specified as the socketFlags parameter and the Socket is configured for in-line reception of out-of-band (OOB) data (using OutOfBandInline) and OOB data is available, only OOB data is returned. When the SocketFlags. Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but not removed from the system buffer.</p>
		/// </remarks>
		public virtual int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP) {
			return InternalSocket.ReceiveFrom(buffer, ref remoteEP);
		}
		/// <summary>
		/// Receives a datagram in a specific location in the data buffer and stores the endpoint.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer, the necessary SocketFlags, and the EndPoint representing the remote host. The offset defaults to 0. The size defaults to the buffer length.</p>
		/// <p>If you use a connectionless protocol, the remoteEP parameter contains the EndPoint associated with the Socket that sent the data. If you use a connection-oriented protocol, remoteEP is left unchanged. You must set the LocalEndPoint property before calling this method. When no incoming data is available, and the Blocking property is false, a SocketException is thrown. When Blocking is true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, then buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p>When the OutOfBand flag is specified as the socketFlags parameter and the Socket is configured for in-line reception of out-of-band (OOB) data (using OutOfBandInline) and OOB data is available, only OOB data is returned. When the SocketFlags. Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but not removed from the system buffer.</p>
		/// </remarks>
		public virtual int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP) {
			return InternalSocket.ReceiveFrom(buffer, socketFlags, ref remoteEP);
		}
		/// <summary>
		/// Receives a datagram in a specific location in the data buffer and stores the endpoint.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size parameter exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>This overload only requires you to provide a receive buffer, the number of bytes you want to receive, the necessary SocketFlags, and the EndPoint representing the remote host. The offset defaults to 0.</p>
		/// <p>If you use a connectionless protocol, the remoteEP parameter contains the EndPoint associated with the Socket that sent the data. If you use a connection-oriented protocol, remoteEP is left unchanged. You must set the LocalEndPoint property before calling this method. When no incoming data is available, and the Blocking property is false, a SocketException is thrown. When Blocking is true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, then buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p>When the OutOfBand flag is specified as the socketFlags parameter and the Socket is configured for in-line reception of out-of-band (OOB) data (using OutOfBandInline) and OOB data is available, only OOB data is returned. When the SocketFlags. Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but not removed from the system buffer.</p>
		/// </remarks>
		public virtual int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP) {
			return InternalSocket.ReceiveFrom(buffer, size, socketFlags, ref remoteEP);
		}
		/// <summary>
		/// Receives a datagram in a specific location in the data buffer and stores the endpoint.
		/// </summary>
		/// <param name="buffer">The storage location for received data.</param>
		/// <param name="offset">The position in the buffer parameter to store the received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
		/// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
		/// <returns>The number of bytes received.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified offset or size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>If you use a connectionless protocol, the remoteEP parameter contains the EndPoint associated with the Socket that sent the data. If you use a connection-oriented protocol, remoteEP is left unchanged. You must set the LocalEndPoint property before calling this method. When no incoming data is available, and the Blocking property is false, a SocketException is thrown. When Blocking is true, this method blocks and waits for data to arrive. For Stream Socket types, if the remote Socket was shut down gracefully, and all data was received, this method immediately returns zero, regardless of the blocking state.</p>
		/// <p>If you are using a message-oriented Socket, and the message is larger than the size of the buffer parameter, then buffer is filled with the first part of the message, and a SocketException is thrown. With unreliable protocols the excess data is lost; with reliable protocols, the data is retained by the service provider.</p>
		/// <p>When the OutOfBand flag is specified as the socketFlags parameter and the Socket is configured for in-line reception of out-of-band (OOB) data (using OutOfBandInline) and OOB data is available, only OOB data is returned. When the SocketFlags. Peek flag is specified as the socketFlags parameter, available data is copied into the receive buffer but not removed from the system buffer.</p>
		/// </remarks>
		public virtual int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP) {
			return InternalSocket.ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP);
		}
		/// <summary>
		/// Sends data to a connected VirtualSocket, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <returns>The number of bytes sent to the VirtualSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>Use Send for connection-oriented protocols only. For connectionless protocols, either use SendTo or call Connect first, and then call Send. This overload only requires you to provide a data buffer. The offset defaults to 0, the size defaults to the buffer length, and SocketFlags value defaults to None.</p>
		/// <p>You must set the LocalEndPoint property of the current instance before calling this method.</p>
		/// </remarks>
		public virtual int Send(byte[] buffer) {
			return InternalSocket.Send(buffer);
		}
		/// <summary>
		/// Sends data to a connected VirtualSocket, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values. </param>
		/// <returns>The number of bytes sent to the VirtualSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>Use Send for connection-oriented protocols only. For connectionless protocols, either use SendTo or call Connect first, and then call Send.</p>
		/// <p>This overload only requires you to provide a data buffer and SocketFlags. The offset defaults to 0, and the size parameter defaults to the buffer length.</p>
		/// <p>You must set the LocalEndPoint property of the current instance before calling this method.</p>
		/// <p>If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent.</p>
		/// <p>If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks.</p>
		/// <p>If you are using a message-oriented Socket, and the size of the buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException.</p>
		/// </remarks>
		public virtual int Send(byte[] buffer, SocketFlags socketFlags) {
			return InternalSocket.Send(buffer, socketFlags);
		}
		/// <summary>
		/// Sends data to a connected VirtualSocket, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="size">The number of bytes to send. </param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values. </param>
		/// <returns>The number of bytes sent to the VirtualSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The size parameter exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>Use Send for connection-oriented protocols only. For connectionless protocols, either use SendTo or call Connect first, and then call Send.</p>
		/// <p>This overload only requires you to provide a data buffer, SocketFlags, and the number bytes to be sent. The offset defaults to 0.</p>
		/// <p>You must set the LocalEndPoint property of the current instance before calling this method.</p>
		/// <p>If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent.</p>
		/// <p>If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks.</p>
		/// <p>If you are using a message-oriented Socket, and the size of the buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException.</p>
		/// </remarks>
		public virtual int Send(byte[] buffer, int size, SocketFlags socketFlags) {
			return InternalSocket.Send(buffer, size, socketFlags);
		}
		/// <summary>
		/// Sends data to a connected VirtualSocket, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="offset">The position in the data buffer to begin sending data.</param>
		/// <param name="size">The number of bytes to send. </param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values. </param>
		/// <returns>The number of bytes sent to the VirtualSocket.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The offset or size parameter exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>Use Send for connection-oriented protocols only. For connectionless protocols, either use SendTo or call Connect first, and then call Send.</p>
		/// <p>This overload gives you the flexibility to specify the Send starting position in the data buffer, the number bytes you are sending, and the necessary SocketFlags.</p>
		/// <p>You must set the LocalEndPoint property of the current instance before calling this method.</p>
		/// <p>If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent.</p>
		/// <p>If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks.</p>
		/// <p>If you are using a message-oriented Socket, and the size of the buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException.</p>
		/// </remarks>
		public virtual int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
			return InternalSocket.Send(buffer, offset, size, socketFlags);
		}
		/// <summary>
		/// Sends data to a specific endpoint, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="remoteEP">The <see cref="EndPoint"/> representing the destination location for the data.</param>
		/// <returns>The number of bytes sent.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br><br>The remoteEP parameter is a null reference (Nothing).</br></exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>If you are using a connection-oriented protocol or a connected Socket using a connectionless protocol, remoteEP overrides the endpoint specified in RemoteEndPoint. If you are using an unconnected Socket with a connectionless protocol, this method sets the LocalEndPoint property of the current instance to a value determined by the protocol. You must subsequently receive data on the LocalEndPoint. This overload only requires you to provide a data buffer, and the remote EndPoint. The offset defaults to 0. The size defaults to the buffer length, and SocketFlags value defaults to None.</p>
		/// </remarks>
		public virtual int SendTo(byte[] buffer, EndPoint remoteEP) {
			return InternalSocket.SendTo(buffer, remoteEP);
		}
		/// <summary>
		/// Sends data to a specific endpoint, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
		/// <param name="remoteEP">The <see cref="EndPoint"/> representing the destination location for the data.</param>
		/// <returns>The number of bytes sent.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br><br>The remoteEP parameter is a null reference (Nothing).</br></exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>If you are using a connection-oriented protocol or a connected Socket using a connectionless protocol, remoteEP overrides the endpoint specified in RemoteEndPoint. If you are using an unconnected Socket with a connectionless protocol, this method sets the LocalEndPoint property of the current instance to a value determined by the protocol. You must subsequently receive data on the LocalEndPoint.</p>
		/// <p>This overload only requires you to provide a data buffer, SocketFlags, and the remote EndPoint. The offset defaults to 0, and size defaults to the buffer length.</p>
		/// <p>Note   If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent. If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks. If you are using a message-oriented Socket, and the size of buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException. If you are using a connection-oriented Socket, remoteEp is ignored.</p>
		/// </remarks>
		public virtual int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP) {
			return InternalSocket.SendTo(buffer, socketFlags, remoteEP);
		}
		/// <summary>
		/// Sends data to a specific endpoint, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="remoteEP">The <see cref="EndPoint"/> representing the destination location for the data.</param>
		/// <returns>The number of bytes sent.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br><br>The remoteEP parameter is a null reference (Nothing).</br></exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified size exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>If you are using a connection-oriented protocol or a connected Socket using a connectionless protocol, remoteEP overrides the endpoint specified in RemoteEndPoint. If you are using an unconnected Socket with a connectionless protocol, this method sets the LocalEndPoint property of the current instance to a value determined by the protocol. You must subsequently receive data on the LocalEndPoint.</p>
		/// <p>This overload only requires you to provide a data buffer, SocketFlags, the number bytes to be sent and the remote EndPoint. The offset defaults to 0.</p>
		/// <p>Note   If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent. If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks. If you are using a message-oriented Socket, and the size of buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException. If you are using a connection-oriented Socket, remoteEp is ignored.</p>
		/// </remarks>
		public virtual int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP) {
			return InternalSocket.SendTo(buffer, size, socketFlags, remoteEP);
		}
		/// <summary>
		/// Sends data to a specific endpoint, starting at the indicated location in the data.
		/// </summary>
		/// <param name="buffer">The data to be sent.</param>
		/// <param name="offset">The position in the data buffer to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
		/// <param name="remoteEP">The <see cref="EndPoint"/> representing the destination location for the data.</param>
		/// <returns>The number of bytes sent.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).<br>-or-</br><br>The remoteEP parameter is a null reference (Nothing).</br></exception>
		/// <exception cref="ArgumentOutOfRangeException">The offset or size parameter exceeds the size of buffer.</exception>
		/// <exception cref="SocketException">An operating system error occurs while accessing the socket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>If you are using a connection-oriented protocol or a connected Socket using a connectionless protocol, remoteEP overrides the endpoint specified in RemoteEndPoint. If you are using an unconnected Socket with a connectionless protocol, this method sets the LocalEndPoint property of the current instance to a value determined by the protocol. You must subsequently receive data on the LocalEndPoint.</p>
		/// <p><b>Note</b>   If you specify the DontRoute flag as the socketflags parameter, the data you are sending will not be routed. If you specify the OutOfBand flag as the socketflags parameter, only out-of-band (OOB) data is sent. If you set the Blocking property to true, and buffer space is not available within the underlying protocol, this method blocks. If you are using a message-oriented Socket, and the size of buffer is greater than the maximum message size of the underlying protocol, no data is sent and Socket will throw a SocketException. If you are using a connection-oriented Socket, remoteEp is ignored.</p>
		/// </remarks>
		public virtual int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP) {
			return InternalSocket.SendTo(buffer, offset, size, socketFlags, remoteEP);
		}
		/// <summary>
		/// Sets the specified option to the specified value.
		/// </summary>
		/// <param name="optionLevel">A <see cref="SocketOptionLevel"/> value. </param>
		/// <param name="optionName">A <see cref="SocketOptionName"/> value.</param>
		/// <param name="optionValue">A byte array representing the value of the option.</param>
		/// <exception cref="SocketException">The VirtualSocket has been closed.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>Socket options determine the behavior of the current Socket. Use this overload to set those Socket options that require a byte array as an option value.<br>Windows 98, Windows NT 4.0 Platform Note:  You must call the Bind method before using AddMembership as the optionName parameter.</br></remarks>
		public virtual void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
			InternalSocket.SetSocketOption(optionLevel, optionName, optionValue);
		}
		/// <summary>
		/// Sets the specified option to the specified value.
		/// </summary>
		/// <param name="optionLevel">A <see cref="SocketOptionLevel"/> value. </param>
		/// <param name="optionName">A <see cref="SocketOptionName"/> value.</param>
		/// <param name="optionValue">A value of the option.</param>
		/// <exception cref="SocketException">The VirtualSocket has been closed.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>Socket options determine the behavior of the current Socket. For an option with a Boolean data type, specify a nonzero value to enable the option, and a zero value to disable the option. For an option with an integer data type, specify the appropriate value. Socket options are grouped by level of protocol support.<br>Windows 98, Windows NT 4.0 Platform Note:  You must call the Bind method before using AddMembership as the optionName parameter.</br></remarks>
		public virtual void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue) {
			InternalSocket.SetSocketOption(optionLevel, optionName, optionValue);
		}
		/// <summary>
		/// Sets the specified option to the specified value.
		/// </summary>
		/// <param name="optionLevel">A <see cref="SocketOptionLevel"/> value. </param>
		/// <param name="optionName">A <see cref="SocketOptionName"/> value.</param>
		/// <param name="optionValue">A LingerOption or MulticastOption containing the value of the option.</param>
		/// <exception cref="SocketException">The VirtualSocket has been closed.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="optionValue"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <remarks>Socket options determine the behavior of the current Socket. Use this overload to set those Socket options that require anything other than an integer or Boolean as an option value. For example, to set the Linger option, you must create an instance of LingerOption and pass it to SetSocketOption as the optionvalue parameter.<br>Windows 98, Windows NT 4.0 Platform Note:  You must call the Bind method before using AddMembership as the optionName parameter.</br></remarks>
		public virtual void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue) {
			InternalSocket.SetSocketOption(optionLevel, optionName, optionValue);
		}
		/// <summary>
		/// Disables sends and receives on a VirtualSocket.
		/// </summary>
		/// <param name="how">The <see cref="SocketShutdown"/> value specifying the operation that will no longer be allowed.</param>
		/// <exception cref="SocketException">An error occurs while closing the VirtualSocket.</exception>
		/// <exception cref="ObjectDisposedException">The VirtualSocket has been closed.</exception>
		/// <remarks>
		/// <p>Setting how to Send, specifies that subsequent calls to Send are not allowed. With TCP sockets, a FIN will be sent after all data is sent and acknowledged by the receiver.</p>
		/// <p>Setting how to Receive, specifies that subsequent calls to Receive are not allowed. This has no effect on lower protocol layers. For TCP sockets, the connection is reset if data is waiting to be received or if more data arrives after the Socket is disabled. For UDP sockets, datagrams are accepted and queued.</p>
		/// <p>Setting how to Both disables both sends and receives as described above.</p>
		/// <p>To finish closing the Socket, a call to Close must be made after the call to Shutdown. You should not attempt to reuse the Socket.</p>
		/// </remarks>
		public virtual void Shutdown(SocketShutdown how) {
			InternalSocket.Shutdown(how);
		}
		/// <summary>Holds the value of the <see cref="InternalSocket"/> property.</summary>
		private Socket m_InternalSocket;
	}
}
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using Org.Mentalis.Security.Ssl.Shared;

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Provides the underlying stream of data for secure network access.
	/// </summary>
	public class SecureNetworkStream : Stream {
		/// <summary>
		/// Creates a new instance of the SecureNetworkStream class for the specified <see cref="SecureSocket"/>.
		/// </summary>
		/// <param name="socket">The SecureSocket that provides the network data. </param>
		/// <exception cref="ArgumentNullException"><paramref name="socket"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="socket"/> is not connected -or- the SocketType property of <paramref name="socket"/> is not SocketType.Stream.</exception>
		/// <exception cref="IOException"><paramref name="socket"/> is a nonblocking socket.</exception>
		public SecureNetworkStream(SecureSocket socket) : this(socket, FileAccess.ReadWrite, false) {}
		/// <summary>
		/// Creates a new instance of the SecureNetworkStream class for the specified <see cref="SecureSocket"/>.
		/// </summary>
		/// <param name="socket">The SecureSocket that provides the network data. </param>
		/// <param name="ownsSocket"><b>true</b> if the socket will be owned by this NetworkStream instance; otherwise, <b>false</b>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="socket"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="socket"/> is not connected -or- the SocketType property of <paramref name="socket"/> is not SocketType.Stream.</exception>
		/// <exception cref="IOException"><paramref name="socket"/> is a nonblocking socket.</exception>
		public SecureNetworkStream(SecureSocket socket, bool ownsSocket) : this(socket, FileAccess.ReadWrite, ownsSocket) {}
		/// <summary>
		/// Creates a new instance of the SecureNetworkStream class for the specified <see cref="SecureSocket"/>.
		/// </summary>
		/// <param name="socket">The SecureSocket that provides the network data. </param>
		/// <param name="access">One of the <see cref="FileAccess"/> values that sets the CanRead and CanWrite properties of the SecureNetworkStream.</param>
		/// <exception cref="ArgumentNullException"><paramref name="socket"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="socket"/> is not connected -or- The SocketType property of socket is not SocketType.Stream.</exception>
		/// <exception cref="IOException"><paramref name="socket"/> is a nonblocking socket.</exception>
		public SecureNetworkStream(SecureSocket socket, FileAccess access) : this(socket, access, false) {}
		/// <summary>
		/// Creates a new instance of the SecureNetworkStream class for the specified <see cref="SecureSocket"/>.
		/// </summary>
		/// <param name="socket">The SecureSocket that provides the network data.</param>
		/// <param name="access">One of the FileAccess values that sets the CanRead and CanWrite properties of the SecureNetworkStream.</param>
		/// <param name="ownsSocket"><b>true</b> if the socket will be owned by this SecureNetworkStream instance; otherwise, <b>false</b>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="socket"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="socket"/> is not connected -or- the SocketType property of socket is not SocketType.Stream.</exception>
		/// <exception cref="IOException"><paramref name="socket"/> is a nonblocking socket.</exception>
		public SecureNetworkStream(SecureSocket socket, FileAccess access, bool ownsSocket) {
			if (socket == null)
				throw new ArgumentNullException();
			if (!socket.Blocking)
				throw new IOException();
			if (!socket.Connected || socket.SocketType != SocketType.Stream)
				throw new ArgumentException();
			m_CanRead = (access == FileAccess.Read || access == FileAccess.ReadWrite);
			m_CanWrite = (access == FileAccess.Write || access == FileAccess.ReadWrite);
			m_OwnsSocket = ownsSocket;
			m_Socket = socket;
		}
		/// <summary>
		/// Gets a value that indicates whether the current stream supports writing.
		/// </summary>
		/// <value><b>true</b> if data can be written to the stream; otherwise, <b>false</b>.</value>
		public override bool CanRead {
			get {
				return m_CanRead;
			}
		}
		/// <summary>
		/// Gets a value that indicates whether the current stream supports writing.
		/// </summary>
		/// <value><b>true</b> if data can be written to the stream; otherwise, <b>false</b>.</value>
		public override bool CanWrite {
			get {
				return m_CanWrite;
			}
		}
		/// <summary>
		/// Gets a value indicating whether the stream supports seeking. This property always returns false.
		/// </summary>
		/// <value><b>false</b> to indicate that SecureNetworkStream cannot seek a specific location in the stream.</value>
		public override bool CanSeek {
			get {
				return false;
			}
		}
		/// <summary>
		/// Flushes data from the stream. This method is reserved for future use.
		/// </summary>
		/// <remarks>
		/// The Flush method implements the Stream.Flush method but, because SecureNetworkStream is not buffered, has no effect on secure network streams. Calling the Flush method will not throw an exception.
		/// </remarks>
		public override void Flush() {}
		/// <summary>
		/// The length of the data available on the stream. This property always throws a NotSupportedException.
		/// </summary>
		/// <value>The length of the data available on the stream. This property is not supported.</value>
		/// <exception cref="NotSupportedException">The Length property is not supported.</exception>
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// Gets or sets the current position in the stream. This property always throws a NotSupportedException.
		/// </summary>
		/// <value>The current position in the stream. This property is not supported.</value>
		/// <exception cref="NotSupportedException">The Position property is not supported.</exception>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// Sets the current position of the stream to the given value. This method always throws a NotSupportedException.
		/// </summary>
		/// <param name="offset">This parameter is not used.</param>
		/// <param name="origin">This parameter is not used.</param>
		/// <returns>The position in the stream. This method is not supported.</returns>
		/// <exception cref="NotSupportedException">The Seek method is not supported.</exception>
		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}
		/// <summary>
		/// Sets the length of the stream. This method always throws a NotSupportedException.
		/// </summary>
		/// <param name="value">This parameter is not used.</param>
		/// <exception cref="NotSupportedException">The SetLength method is not supported.</exception>
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}
		/// <summary>
		/// Gets the underlying network connection.
		/// </summary>
		/// <value>A <see cref="SecureSocket"/> that represents the underlying network connection.</value>
		protected SecureSocket Socket {
			get {
				return m_Socket;
			}
		}
		/// <summary>
		/// Reads data from the stream.
		/// </summary>
		/// <param name="buffer">The location in memory to store data read from the stream.</param>
		/// <param name="offset">The location in the buffer to begin storing the data to.</param>
		/// <param name="size">The number of bytes to read from the stream.</param>
		/// <returns>The number of bytes read from the stream.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="offset"/> or <paramref name="size"/> exceeds the size of <paramref name="buffer"/>.</exception>
		/// <exception cref="IOException">There is a failure while reading from the network.</exception>
		public override int Read(byte[] buffer, int offset, int size) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset > buffer.Length || size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException();
			if (Socket == null)
				throw new IOException();
			try {
				return Socket.Receive(buffer, offset, size, SocketFlags.None);
			} catch (Exception e) {
				throw new IOException("An I/O exception occurred.", e);
			}
		}
		/// <summary>
		/// Writes data to the stream.
		/// </summary>
		/// <param name="buffer">The data to write to the stream.</param>
		/// <param name="offset">The location in the buffer to start writing data from.</param>
		/// <param name="size">The number of bytes to write to the stream.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="offset"/> or <paramref name="size"/> exceeds the size of <paramref name="buffer"/>.</exception>
		/// <exception cref="IOException">There is a failure while writing to the network.</exception>
		public override void Write(byte[] buffer, int offset, int size) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset > buffer.Length || size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException();
			if (Socket == null)
				throw new IOException();
			try {
				Socket.Send(buffer, offset, size, SocketFlags.None);
			} catch (Exception e) {
				throw new IOException("An I/O exception occurred.", e);
			}
		}
		/// <summary>
		/// Changes the security protocol. This method can only be used to 'upgrade' a connection from no-security to either SSL or TLS.
		/// </summary>
		/// <param name="options">The new <see cref="SecurityOptions"/> parameters.</param>
		/// <exception cref="SecurityException">An error occurs while changing the security protocol.</exception>
		/// <remarks>
		/// Programs should only call this method if there is no active <see cref="Read"/> or <see cref="Write"/>!
		/// </remarks>
		public void ChangeSecurityProtocol(SecurityOptions options) {
			Socket.ChangeSecurityProtocol(options);
		}
		/// <summary>
		/// Closes the stream and optionally closes the underlying <see cref="SecureSocket"/>.
		/// </summary>
		/// <remarks>
		/// The Close method frees resources used by the SecureNetworkStream instance and, if the SecureNetworkStream owns the underlying socket, closes the underlying socket.
		/// </remarks>
		public override void Close() {
			if (m_OwnsSocket) {
				try {
					Socket.Shutdown(SocketShutdown.Both);
				} catch {
				} finally {
					Socket.Close();
				}
			}
		}
		/// <summary>
		/// Begins an asynchronous read from a stream.
		/// </summary>
		/// <param name="buffer">The location in memory that stores the data from the stream.</param>
		/// <param name="offset">The location in buffer to begin storing the data to.</param>
		/// <param name="size">The maximum number of bytes to read.</param>
		/// <param name="callback">The delegate to call when the asynchronous call is complete.</param>
		/// <param name="state">An object containing additional information supplied by the client.</param>
		/// <returns>An <see cref="IAsyncResult"/> representing the asynchronous call.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="offset"/> or <paramref name="size"/> exceeds the size of <paramref name="buffer"/>.</exception>
		/// <exception cref="IOException">There is a failure while reading from the network.</exception>
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset > buffer.Length || size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException();
			if (Socket == null)
				throw new IOException();
			try {
				return Socket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
			} catch (Exception e) {
				throw new IOException("An I/O exception occurred.", e);
			}
		}
		/// <summary>
		/// Handles the end of an asynchronous read.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> representing an asynchronous call. </param>
		/// <returns>The number of bytes read from the stream.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">There is a failure while reading from the network.</exception>
		public override int EndRead(IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException();
			if (Socket == null)
				throw new IOException();
			try {
				return Socket.EndReceive(asyncResult);
			} catch (Exception e) {
				throw new IOException("An I/O exception occurred.", e);
			}
		}
		/// <summary>
		/// Begins an asynchronous write to a stream.
		/// </summary>
		/// <param name="buffer">The location in memory that holds the data to send.</param>
		/// <param name="offset">The location in buffer to begin sending the data.</param>
		/// <param name="size">The size of buffer.</param>
		/// <param name="callback">The delegate to call when the asynchronous call is complete.</param>
		/// <param name="state">An object containing additional information supplied by the client.</param>
		/// <returns>An <see cref="IAsyncResult"/> representing the asynchronous call.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="offset"/> or <paramref name="size"/> exceeds the size of <paramref name="buffer"/>.</exception>
		/// <exception cref="IOException">There is a failure while writing to the network.</exception>
		// Thanks go out to Martin Plante for notifying us about a bug in this method.
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset > buffer.Length || size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException();
			if (Socket == null)
				throw new IOException();
			if (WriteResult != null)
				throw new IOException();
			TransferItem localResult = new TransferItem(new byte[size], 0, size, new AsyncResult(callback, state, null), DataType.ApplicationData);
			WriteResult = localResult;
			Array.Copy(buffer, offset, localResult.Buffer, 0, size);
			try {
				Socket.BeginSend(localResult.Buffer, 0, size, SocketFlags.None, new AsyncCallback(OnBytesSent), (int)0);
				return localResult.AsyncResult;
			} catch {
				throw new IOException();
			}
		}
		/// <summary>
		/// Called when the bytes have been sent to the remote server
		/// </summary>
		/// <param name="asyncResult">The <see cref="IAsyncResult"/> representing the asynchronous call.</param>
		private void OnBytesSent(IAsyncResult asyncResult) {
			try {
				int sent = Socket.EndSend(asyncResult);
				sent += (int)asyncResult.AsyncState;
				if (sent == WriteResult.Buffer.Length) {
					OnWriteComplete(null);
				} else {
					Socket.BeginSend(WriteResult.Buffer, sent, WriteResult.Buffer.Length - sent, SocketFlags.None, new AsyncCallback(OnBytesSent), sent);
				}
			} catch (Exception e) {
				OnWriteComplete(e);
			}
		}
		/// <summary>
		/// Called when all bytes have been sent to the remote host, or when a network error occurred.
		/// </summary>
		/// <param name="e">The error that occurred.</param>
		private void OnWriteComplete(Exception e) {
			if (WriteResult.AsyncResult != null) {
				WriteResult.AsyncResult.AsyncException = e;
				WriteResult.AsyncResult.Notify();
			}
		}
		/// <summary>
		/// Handles the end of an asynchronous write.
		/// </summary>
		/// <param name="asyncResult">The <see cref="IAsyncResult"/> representing the asynchronous call.</param>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The <paramref name="asyncResult"/> parameter was not returned by a call to the BeginWrite method.</exception>
		/// <exception cref="IOException">An error occurs while writing to the network.</exception>
		public override void EndWrite(IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException();
			if (asyncResult != WriteResult.AsyncResult)
				throw new ArgumentException();
			if (Socket == null)
				throw new IOException();
			WriteResult = null;
			if (((AsyncResult)asyncResult).AsyncException != null)
				throw new IOException();
		}
		/// <summary>
		/// Holds the <see cref="TransferItem"/> object returned by BeginWrite.
		/// </summary>
		/// <value>A <see cref="TransferItem"/> object.</value>
		internal TransferItem WriteResult {
			get {
				return m_WriteResult;
			}
			set {
				m_WriteResult = value;
			}
		}
		/// <summary>
		/// Gets a value indicating whether data is available on the stream to be read.
		/// </summary>
		/// <value><b>true</b> if data is available on the stream to be read; otherwise, <b>false</b>.</value>
		public virtual bool DataAvailable {
			get {
				if (Socket == null)
					return false;
				try {
					return Socket.Available > 0;
				} catch {
					return false;
				}
			}
		}
		/// <summary>Holds the value of the <see cref="WriteResult"/> property</summary>
		private TransferItem m_WriteResult = null;
		/// <summary><b>true</b> if the SecureNetworkStream owns the SecureSocket, <b>false</b> otherwise.</summary>
		private bool m_OwnsSocket;
		/// <summary>Holds the value of the <see cref="CanRead"/> property</summary>
		private bool m_CanRead;
		/// <summary>Holds the value of the <see cref="CanWrite"/> property</summary>
		private bool m_CanWrite;
		/// <summary>Holds the value of the <see cref="Socket"/> property</summary>
		private SecureSocket m_Socket;
	}
}
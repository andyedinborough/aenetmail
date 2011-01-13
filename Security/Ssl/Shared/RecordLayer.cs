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
using System.Security.Cryptography;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Ssl3;
using Org.Mentalis.Security.Ssl.Tls1;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.IO;

namespace Org.Mentalis.Security.Ssl.Shared {
	internal class RecordLayer : IDisposable {
		public RecordLayer(SocketController controller, HandshakeLayer handshakeLayer) {
			m_IsDisposed = false;
			m_Controller = controller;
			m_HandshakeLayer = handshakeLayer;
			m_IncompleteMessage = new byte[0];
			ChangeLocalState(null, null, null);
			ChangeRemoteState(null, null, null);
/*			if (options.Entity == ConnectionEnd.Server) {
				if (options.Protocol == SecureProtocol.Tls1) {
					m_HandshakeLayer = new Tls1ServerHandshakeLayer(this, options);
				} else if (options.Protocol == SecureProtocol.Ssl3) {
					m_HandshakeLayer = new Ssl3ServerHandshakeLayer(this, options);
				} else {
					throw new NotSupportedException();
				}
			} else {
				if (options.Protocol == SecureProtocol.Tls1) {
					m_HandshakeLayer = new Tls1ClientHandshakeLayer(this, options);
				} else if (options.Protocol == SecureProtocol.Ssl3) {
					m_HandshakeLayer = new Ssl3ClientHandshakeLayer(this, options);
				} else {
					throw new NotSupportedException();
				}
			}*/
		}
		public void ChangeLocalState(CompressionAlgorithm compressor, ICryptoTransform encryptor, KeyedHashAlgorithm localHasher) {
			m_LocalCompressor = compressor;
			m_BulkEncryption = encryptor;
			m_LocalHasher = localHasher;
			m_OutputSequenceNumber = 0;
		}
		public void ChangeRemoteState(CompressionAlgorithm decompressor, ICryptoTransform decryptor, KeyedHashAlgorithm remoteHasher) {
			m_RemoteCompressor = decompressor;
			m_BulkDecryption = decryptor;
			m_RemoteHasher = remoteHasher;
			m_InputSequenceNumber = 0;
		}
/*		public static void PrintBytes(byte[] array, int offset, int size) {
			for(int i = 0; i < size; i++) {
				Console.Write(array[offset + i].ToString() + "_");
			}
			Console.WriteLine("__");
		}*/
		protected byte[] InternalEncryptBytes2(byte[] buffer, int offset, int size, ContentType type) { // only accepts sizes of less than 16Kb; does not do any error checking
			byte[] ret = new byte[GetEncryptedLength(size) + 5];
			ret[0] = (byte)type;
			ret[1] = m_HandshakeLayer.GetVersion().major;
			ret[2] = m_HandshakeLayer.GetVersion().minor;
			ret[3] = (byte)((ret.Length - 5) / 256);
			ret[4] = (byte)((ret.Length - 5) % 256);

			byte[] mac = null;
			try {
				if (m_LocalHasher == null) { // copy the message
					Array.Copy(buffer, offset, ret, 5, size);
				} else { // encrypt the message and MAC
					//TODO: redundant initialize?
					m_LocalHasher.Initialize();
					mac = new byte[8 + 5];
					Array.Copy(GetULongBytes(m_OutputSequenceNumber), 0, mac, 0, 8);
					mac[8] = (byte)type;
					if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
						mac[9] = ret[1];
						mac[10] = ret[2];
						mac[11] = (byte)(size / 256);
						mac[12] = (byte)(size % 256);
						m_LocalHasher.TransformBlock(mac, 0, 13, mac, 0);
					} else { // SSL3
						mac[9] = (byte)(size / 256);
						mac[10] = (byte)(size % 256);
						m_LocalHasher.TransformBlock(mac, 0, 11, mac, 0);
					}
					m_LocalHasher.TransformFinalBlock(buffer, offset, size);
					mac = m_LocalHasher.Hash;
					// encrypt the message
					if (m_BulkEncryption.OutputBlockSize == 1) { // is stream cipher?
						m_BulkEncryption.TransformBlock(buffer, offset, size, ret, 5);
						m_BulkEncryption.TransformBlock(mac, 0, mac.Length, ret, size + 5);
					} else { // cipher is block cipher
						int obs = m_BulkEncryption.OutputBlockSize;
						byte padding = (byte)((obs - (size + mac.Length + 1) % obs) % obs);

						int messTrailer = (size % obs);
						int pos = 5 + size - messTrailer;
						if (size - messTrailer != 0)
							m_BulkEncryption.TransformBlock(buffer, offset, size - messTrailer, ret, 5);
						
						byte[] trailer = new byte[messTrailer + m_LocalHasher.HashSize / 8 + padding + 1];
						if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
							for(int i = messTrailer + mac.Length; i < trailer.Length; i++) {
								trailer[i] = padding;
							}
						} else {
							m_HandshakeLayer.RNG.GetBytes(trailer);
							ret[ret.Length - 1] = padding;
						}
						if (messTrailer > 0)
							Array.Copy(buffer, offset + size - messTrailer, trailer, 0, messTrailer);
						Array.Copy(mac, 0, trailer, messTrailer, mac.Length);
						m_BulkEncryption.TransformBlock(trailer, 0, trailer.Length, ret, pos);
					}
				}
			} catch (Exception e) {
				throw new SslException(e, AlertDescription.InternalError, "An exception occurred");
			}
			m_OutputSequenceNumber++;
			return ret;
		}
		private int GetEncryptedLength(int size) { // returns length of encrypted data [without message header length]
			if (m_LocalHasher != null) {
				if (m_BulkEncryption.OutputBlockSize == 1) { // is stream cipher?
					return size + m_LocalHasher.HashSize / 8;
				} else { // cipher is block cipher
					int obs = m_BulkEncryption.OutputBlockSize;
					byte padding = (byte)((obs - (size + m_LocalHasher.HashSize / 8 + 1) % obs) % obs);
					return size + m_LocalHasher.HashSize / 8 + padding + 1;
				}
			} else {
				return size;
			}
		}
		protected byte[] InternalEncryptBytes(byte[] buffer, int offset, int size, ContentType type) { // only accepts sizes of less than 16Kb; does not do any error checking
			byte[] bytes = new byte[size];
			Array.Copy(buffer, offset, bytes, 0, size);
			RecordMessage message = new RecordMessage(MessageType.PlainText, type, m_HandshakeLayer.GetVersion(), bytes);
			WrapMessage(message);
			return message.ToBytes();
		}
		protected void WrapMessage(RecordMessage message) {
			if (message.length != message.fragment.Length)
				throw new SslException(AlertDescription.IllegalParameter, "Message length is invalid.");
			byte[] mac = null;
			try {
				// compress the message
				if (m_LocalCompressor != null) {
					message.fragment = m_LocalCompressor.Compress(message.fragment);
					message.length = (ushort)message.fragment.Length;
				}
				// encrypt the message and MAC
				if (m_LocalHasher != null) {
					// calculate the MAC
					mac = GetULongBytes(m_OutputSequenceNumber);
					m_LocalHasher.Initialize();
					m_LocalHasher.TransformBlock(mac, 0, mac.Length, mac, 0);	// seq_num + ..
					mac = message.ToBytes();
					if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
						m_LocalHasher.TransformFinalBlock(mac, 0, mac.Length);		// .. + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length + TLSCompressed.fragment
					} else if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Ssl3) {
						m_LocalHasher.TransformBlock(mac, 0, 1, mac, 0); // type
						m_LocalHasher.TransformFinalBlock(mac, 3, mac.Length - 3); // length + fragment
					} else {
						throw new NotSupportedException("Only SSL3 and TLS1 are supported");
					}
					mac = m_LocalHasher.Hash;
					// encrypt the message
					if (m_BulkEncryption.OutputBlockSize == 1) { // is stream cipher?
						byte[] ret = new byte[message.length + mac.Length];
						m_BulkEncryption.TransformBlock(message.fragment, 0, message.length, ret, 0);
						m_BulkEncryption.TransformBlock(mac, 0, mac.Length, ret, message.length);
						message.fragment = ret;
					} else { // cipher is block cipher
						int obs = m_BulkEncryption.OutputBlockSize;
						byte padding = (byte)((obs - (message.length + mac.Length + 1) % obs) % obs);
						byte[] ret = new byte[message.length + mac.Length + padding + 1];
						Array.Copy(message.fragment, 0, ret, 0, message.length);
						Array.Copy(mac, 0, ret, message.length, mac.Length);
						if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
							for(int i = message.length + mac.Length; i < ret.Length; i++) {
								ret[i] = padding;
							}
						} else {
							byte[] buffer = new byte[ret.Length - message.length - mac.Length];
							m_HandshakeLayer.RNG.GetBytes(buffer);
							Array.Copy(buffer, 0, ret, message.length + mac.Length, buffer.Length);
							ret[ret.Length - 1] = padding;
						}
						m_BulkEncryption.TransformBlock(ret, 0, ret.Length, ret, 0);
						message.fragment = ret;
					}
					message.length = (ushort)message.fragment.Length;
				}
			} catch (Exception e) {
				throw new SslException(e, AlertDescription.InternalError, "An exception occurred");
			}
			// final adjustments
			message.messageType = MessageType.Encrypted;
			m_OutputSequenceNumber++;
		} //*/
		protected void UnwrapMessage(RecordMessage message) {
			if (message.length != message.fragment.Length)
				throw new SslException(AlertDescription.IllegalParameter, "Message length is invalid.");
			byte[] remoteMac = null, decrypted = null, localMac = null;
			bool cipherError = false;
			// decrypt and verify the message
			if (m_BulkDecryption != null) {
				if (message.length <= m_RemoteHasher.HashSize / 8)
					throw new SslException(AlertDescription.DecodeError, "Message is too small.");
				if (message.length % m_BulkDecryption.OutputBlockSize != 0)
					throw new SslException(AlertDescription.DecryptError, "Message length is invalid.");
				// decrypt the message
				if (m_BulkDecryption.OutputBlockSize == 1) { // is stream cipher?
					decrypted = new byte[message.length];
					m_BulkDecryption.TransformBlock(message.fragment, 0, message.length, decrypted, 0);
					remoteMac = new byte[m_RemoteHasher.HashSize / 8];
					Array.Copy(decrypted, message.length - remoteMac.Length, remoteMac, 0, remoteMac.Length);
					message.fragment = new byte[decrypted.Length - remoteMac.Length];
					Array.Copy(decrypted, 0, message.fragment, 0, message.fragment.Length);
					message.length = (ushort)message.fragment.Length;
				} else { // cipher is block cipher
					decrypted = new byte[message.fragment.Length];
					m_BulkDecryption.TransformBlock(message.fragment, 0, decrypted.Length, decrypted, 0);
					byte padding = decrypted[decrypted.Length - 1];
					if (message.length < padding + m_RemoteHasher.HashSize / 8 + 1) {
						cipherError = true;
						remoteMac = new byte[m_RemoteHasher.HashSize / 8];
					} else {
						int realSize = (message.length - padding) - 1;
						remoteMac = new byte[m_RemoteHasher.HashSize / 8];
						Array.Copy(decrypted, realSize - remoteMac.Length, remoteMac, 0, remoteMac.Length);
						message.fragment = new byte[realSize - remoteMac.Length];
						Array.Copy(decrypted, 0, message.fragment, 0, message.fragment.Length);
						message.length = (ushort)message.fragment.Length;
						if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
							// check padding
							for(int i = realSize; i < decrypted.Length; i++) {
								if (decrypted[i] != padding) {
									cipherError = true;
								}
							}
						}
					}
				}
				// calculate the MAC
				localMac = GetULongBytes(m_InputSequenceNumber);
				m_RemoteHasher.Initialize();
				m_RemoteHasher.TransformBlock(localMac, 0, localMac.Length, localMac, 0);	// seq_num + ..
				localMac = message.ToBytes();
				if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Tls1) {
					m_RemoteHasher.TransformFinalBlock(localMac, 0, localMac.Length);		// .. + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length + TLSCompressed.fragment
				} else if (m_HandshakeLayer.GetProtocol() == SecureProtocol.Ssl3) {
					m_RemoteHasher.TransformBlock(localMac, 0, 1, localMac, 0); // type
					m_RemoteHasher.TransformFinalBlock(localMac, 3, localMac.Length - 3); // length + fragment
				} else {
					throw new NotSupportedException("Only SSL3 and TLS1 are supported");
				}
				localMac = m_RemoteHasher.Hash;
				// compare MACs
				for(int i = 0; i < remoteMac.Length; i++) {
					if (remoteMac[i] != localMac[i]) {
						cipherError = true;
					}
				}
				// throw cipher error, if necessary
				if (cipherError)
					throw new SslException(AlertDescription.BadRecordMac, "An error occurred during the decryption and verification process.");
			}
			// decompress the message
			if (m_RemoteCompressor != null) {
				message.fragment = m_RemoteCompressor.Decompress(message.fragment);
				message.length = (ushort)message.fragment.Length;
			}
			// final adjustments
			message.messageType = MessageType.PlainText;
			m_InputSequenceNumber++;
		}
		protected byte[] GetULongBytes(ulong number) {
			byte[] ret = BitConverter.GetBytes(number);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(ret); // TLS uses big endian [network] byte order
			return ret;
		}
		public byte[] EncryptBytes(byte[] buffer, int offset, int size, ContentType type) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset + size > buffer.Length || size < 0)
				throw new ArgumentException();
			//TODO: als buffer kleiner dan max length => geen memorystream
			MemoryStream ms = new MemoryStream(size + (size / m_MaxMessageLength + 1) * 25);
			byte[] message;
			int encrypted = 0;
			while(encrypted < size) {
				if (encrypted + m_MaxMessageLength > size) {
					//message = InternalEncryptBytes2(buffer, offset + encrypted, size - encrypted, type);
					message = InternalEncryptBytes(buffer, offset + encrypted, size - encrypted, type);
					ms.Write(message, 0, message.Length);
				} else {
					message = InternalEncryptBytes(buffer, offset + encrypted, m_MaxMessageLength, type);
					ms.Write(message, 0, message.Length);
				}
				encrypted += m_MaxMessageLength;
			}
			return ms.ToArray();
		}
		protected bool IsRecordMessageComplete(byte[] buffer, int offset) {
			if (buffer.Length < offset + 6)
				return false;
			int size = buffer[offset + 3] * 256 + buffer[offset + 4];
			return buffer.Length >= offset + 5 + size;
		}
		public SslRecordStatus ProcessBytes(byte[] buffer, int offset, int size) {
			if (buffer == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset + size > buffer.Length || size <= 0)
				throw new ArgumentException();
			SslRecordStatus ret = new SslRecordStatus();
			ret.Status = SslStatus.MessageIncomplete;
			MemoryStream decrypted = new MemoryStream();
			MemoryStream protocol = new MemoryStream();
			// copy the new bytes and the old bytes in one buffer
			byte[] fullbuffer = new byte[m_IncompleteMessage.Length + size];
			Array.Copy(m_IncompleteMessage, 0, fullbuffer, 0, m_IncompleteMessage.Length);
			Array.Copy(buffer, offset, fullbuffer, m_IncompleteMessage.Length, size);
			// extract all record messages, if any, and process them
			int recordSize = 0;
			int recordLength;
			while(IsRecordMessageComplete(fullbuffer, recordSize)) {
				RecordMessage message = new RecordMessage(fullbuffer, recordSize);
				recordLength = message.length + 5;
				UnwrapMessage(message); // decrypt and verify message
				// process message
				if (message.contentType == ContentType.ApplicationData) {
					if (!m_HandshakeLayer.IsNegotiating()) {
						decrypted.Write(message.fragment, 0, message.fragment.Length);
					} else {
						throw new SslException(AlertDescription.UnexpectedMessage, "The handshake procedure was not completed successfully before application data was received.");
					}
					ret.Status = SslStatus.OK;
				} else { // handshake message or change cipher spec message
					SslHandshakeStatus status = m_HandshakeLayer.ProcessMessages(message);
					if (status.Message != null)
						protocol.Write(status.Message, 0, status.Message.Length);
					ret.Status = status.Status;
				}
				recordSize += recordLength;
			}
			// copy remaining data [incomplete record]
			if (recordSize > 0) {
				m_IncompleteMessage = new byte[fullbuffer.Length - recordSize];
				Array.Copy(fullbuffer, recordSize, m_IncompleteMessage, 0, m_IncompleteMessage.Length);
			} else {
				m_IncompleteMessage = fullbuffer;
			}
			if (decrypted.Length > 0) {
				ret.Decrypted = decrypted.ToArray();
			}
			decrypted.Close();
			if (protocol.Length > 0) {
				ret.Buffer = protocol.ToArray();
			}
			protocol.Close();
			return ret;
		}
		public SslRecordStatus ProcessSsl2Hello(byte[] hello) {
			SslHandshakeStatus hs = m_HandshakeLayer.ProcessSsl2Hello(hello);
			return new SslRecordStatus(hs.Status, hs.Message, null);
		}
		public byte[] GetControlBytes(ControlType type) { // returns null if no bytes should be sent
			return m_HandshakeLayer.GetControlBytes(type);
		}
		public bool IsNegotiating() {
			return m_HandshakeLayer.IsNegotiating();
		}
		public SecureSocket Parent {
			get {
				return m_Controller.Parent;
			}
		}
		public void Dispose() {
			if (!m_IsDisposed) {
				m_IsDisposed = true;
				if (m_BulkEncryption != null)
					m_BulkEncryption.Dispose();
				if (m_BulkDecryption != null)
					m_BulkDecryption.Dispose();
				if (m_LocalHasher != null)
					m_LocalHasher.Clear();
				if (m_RemoteHasher != null)
					m_RemoteHasher.Clear();
				m_InputSequenceNumber = 0;
				m_OutputSequenceNumber = 0;
				m_HandshakeLayer.Dispose();
			}
		}
		public SslAlgorithms ActiveEncryption {
			get {
				return m_HandshakeLayer.ActiveEncryption;
			}
		}
		public Certificate RemoteCertificate {
			get {
				return m_HandshakeLayer.RemoteCertificate;
			}
		}
		internal HandshakeLayer HandshakeLayer {
			get {
				return m_HandshakeLayer;
			}
			set {
				m_HandshakeLayer = value;
			}
		}
		private CompressionAlgorithm m_LocalCompressor;
		private CompressionAlgorithm m_RemoteCompressor;
		private ICryptoTransform m_BulkEncryption;
		private ICryptoTransform m_BulkDecryption;
		private KeyedHashAlgorithm m_LocalHasher;
		private KeyedHashAlgorithm m_RemoteHasher;
		private ulong m_InputSequenceNumber;		// #records received from other side
		private ulong m_OutputSequenceNumber;		// #records sent to other side
		private const int m_MaxMessageLength = 16384;
		private byte[] m_IncompleteMessage;			// should *NEVER* be null
		private HandshakeLayer m_HandshakeLayer;
		private SocketController m_Controller;
		private bool m_IsDisposed;
	}
}
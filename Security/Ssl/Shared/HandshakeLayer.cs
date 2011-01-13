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
using Org.Mentalis.Security.Cryptography;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl;
using System.Collections;
using System.Text;
using System.IO;

namespace Org.Mentalis.Security.Ssl.Shared {
	/*
      Client                                               Server

      ClientHello                  -------->
                                                      ServerHello
                                                     Certificate*
                                               ServerKeyExchange*
                                              CertificateRequest*
                                   <--------      ServerHelloDone
      Certificate*
      ClientKeyExchange
      CertificateVerify*
      [ChangeCipherSpec]
      Finished                     -------->
                                               [ChangeCipherSpec]
                                   <--------             Finished
      Application Data             <------->     Application Data
	*/
	internal abstract class HandshakeLayer : IDisposable {
		public HandshakeLayer(RecordLayer recordLayer, SecurityOptions options) {
			m_Disposed = false;
			m_Options = options;
			m_IsNegotiating = true;
			m_RNG = new RNGCryptoServiceProvider();
			m_RecordLayer = recordLayer;
			m_State = HandshakeType.Nothing;
			m_IncompleteMessage = new byte[0];
			m_LocalMD5Hash = new MD5CryptoServiceProvider();
			m_LocalSHA1Hash = new SHA1CryptoServiceProvider();
			m_RemoteMD5Hash = new MD5CryptoServiceProvider();
			m_RemoteSHA1Hash = new SHA1CryptoServiceProvider();
			m_CertSignHash = new MD5SHA1CryptoServiceProvider();
			m_CertSignHash.Protocol = this.GetProtocol();
			if (options.Entity == ConnectionEnd.Server && ((int)options.Flags & (int)SecurityFlags.MutualAuthentication) != 0)
				m_MutualAuthentication = true;
			else
				m_MutualAuthentication = false;
		}
		public HandshakeLayer(HandshakeLayer handshakeLayer) {
			m_Disposed = false;
			m_RecordLayer = handshakeLayer.m_RecordLayer;
			m_Options = handshakeLayer.m_Options;
			m_IsNegotiating = handshakeLayer.m_IsNegotiating;
			m_RNG = handshakeLayer.m_RNG;
			m_State = handshakeLayer.m_State;
			m_IncompleteMessage = handshakeLayer.m_IncompleteMessage;
			m_LocalMD5Hash = handshakeLayer.m_LocalMD5Hash;
			m_LocalSHA1Hash = handshakeLayer.m_LocalSHA1Hash;
			m_RemoteMD5Hash = handshakeLayer.m_RemoteMD5Hash;
			m_RemoteSHA1Hash = handshakeLayer.m_RemoteSHA1Hash;
			m_CertSignHash = handshakeLayer.m_CertSignHash;
			m_CertSignHash.Protocol = this.GetProtocol();
			m_MutualAuthentication = handshakeLayer.m_MutualAuthentication;
			m_ClientTime = handshakeLayer.m_ClientTime;
			m_ClientRandom = handshakeLayer.m_ClientRandom;
			handshakeLayer.Dispose(false);
		}
		// processes Handshake & ChangeCipherSpec messages
		public SslHandshakeStatus ProcessMessages(RecordMessage message) {
			if (message == null)
				throw new ArgumentNullException();
			SslHandshakeStatus ret;
			if (message.contentType == ContentType.ChangeCipherSpec) {
				ret = ProcessChangeCipherSpec(message);
				m_State = HandshakeType.ChangeCipherSpec;
			} else if (message.contentType == ContentType.Handshake) {
				ret = new SslHandshakeStatus();
				// copy the new bytes and the old bytes in one buffer
				MemoryStream ms = new MemoryStream();
				byte[] fullbuffer = new byte[m_IncompleteMessage.Length + message.length];
				Array.Copy(m_IncompleteMessage, 0, fullbuffer, 0, m_IncompleteMessage.Length);
				Array.Copy(message.fragment, 0, fullbuffer, m_IncompleteMessage.Length, message.length);
				// loop through all messages in buffer, if any
				int offset = 0;
				HandshakeMessage hm = GetHandshakeMessage(fullbuffer, offset);
				while(hm != null) {
					offset += hm.fragment.Length + 4;
					SslHandshakeStatus status = ProcessMessage(hm);
					if (status.Message != null) {
						ms.Write(status.Message, 0, status.Message.Length);
					}
					ret.Status = status.Status;
					// go to next message
					m_State = hm.type;
					hm = GetHandshakeMessage(fullbuffer, offset);
				}	
				if (offset > 0) {
					m_IncompleteMessage = new byte[fullbuffer.Length - offset];
					Array.Copy(fullbuffer, offset, m_IncompleteMessage, 0, m_IncompleteMessage.Length);
				} else {
					m_IncompleteMessage = fullbuffer;
				}
				if (ms.Length > 0) {
					ret.Message = ms.ToArray();
				}
				ms.Close();
			} else { // message.contentType == ContentType.Alert
				ret = ProcessAlert(message);
			}
			return ret;
		}
		protected SslHandshakeStatus ProcessCertificate(HandshakeMessage message, bool client) {
			if (client) {
				if (m_State != HandshakeType.ServerHello)
					throw new SslException(AlertDescription.UnexpectedMessage, "Certificate message must be preceded by a ServerHello message.");
			} else { // server
				if (m_State != HandshakeType.ClientHello)
					throw new SslException(AlertDescription.UnexpectedMessage, "Certificate message must be preceded by a ClientHello message.");
			}
			UpdateHashes(message, HashUpdate.All); // input message
			Certificate[] certs = null;
			try {
				certs = ParseCertificateList(message.fragment);
				if (certs.Length == 0) {
					if (!m_MutualAuthentication)
						return new SslHandshakeStatus(SslStatus.MessageIncomplete, null);
				}
			} catch (SslException t) {
				throw t;
			} catch (Exception f) {
				throw new SslException(f, AlertDescription.InternalError, "The Certificate message is invalid.");
			}
			CertificateChain chain = null;
			m_RemoteCertificate = null;
			if (certs.Length != 0) {
				m_RemoteCertificate = certs[0];
				if (m_RemoteCertificate.GetPublicKeyLength() < 512) {
					throw new SslException(AlertDescription.HandshakeFailure, "The pulic key should be at least 512 bits.");
				}
				CertificateStore cs = new CertificateStore(certs);
				for(int i = 0; i < certs.Length; i++) {
					certs[i].Store = cs;
				}
				chain = new CertificateChain(m_RemoteCertificate, cs);
			}
			VerifyChain(chain, client);
			return new SslHandshakeStatus(SslStatus.MessageIncomplete, null);
		}
		protected void VerifyChain(CertificateChain chain, bool client) {
			VerifyEventArgs e = new VerifyEventArgs();
			switch(m_Options.VerificationType) {
				case CredentialVerification.Manual:
					try {
						m_Options.Verifier(Parent, m_RemoteCertificate, chain, e);
					} catch (Exception de) {
						throw new SslException(de, AlertDescription.InternalError, "The code inside the CertVerifyEventHandler delegate threw an exception.");
					}
					break;
				case CredentialVerification.Auto:
					if (chain != null)
						e.Valid = (chain.VerifyChain(m_Options.CommonName, client ? AuthType.Client : AuthType.Server) == CertificateStatus.ValidCertificate);
					else
						e.Valid = false;
					break;
				case CredentialVerification.AutoWithoutCName:
					if (chain != null)
						e.Valid = (chain.VerifyChain(m_Options.CommonName, client ? AuthType.Client : AuthType.Server, VerificationFlags.IgnoreInvalidName) == CertificateStatus.ValidCertificate);
					else
						e.Valid = false;
					break;
				case CredentialVerification.None:
				default:
					e.Valid = true;
					break;
			}
			if (!e.Valid) {
				throw new SslException(AlertDescription.CertificateUnknown, "The certificate could not be verified.");
			}
		}
		protected Certificate[] ParseCertificateList(byte[] list) {
			Queue queue = new Queue();
			int offset = 3;
			while(offset < list.Length) {
				int length = list[offset] * 65536 + list[offset + 1] * 256 + list[offset + 2];
				queue.Enqueue(Certificate.CreateFromCerFile(list, offset + 3, length));
				offset += length + 3;
			}
			Certificate[] certs = new Certificate[queue.Count];
			offset = 0;
			while(queue.Count > 0) {
				certs[offset] = (Certificate)queue.Dequeue();
				offset++;
			}
			return certs;
		}
		protected SslHandshakeStatus ProcessAlert(RecordMessage message) {
			if (message.length != 2 || message.fragment.Length != 2)
				throw new SslException(AlertDescription.RecordOverflow, "The alert message is invalid.");
			try {
				AlertLevel level = (AlertLevel)message.fragment[0];
				AlertDescription description = (AlertDescription)message.fragment[1];
				if (level == AlertLevel.Fatal)
					throw new SslException(description, "The other side has sent a failure alert.");
				SslHandshakeStatus ret;
				if (description == AlertDescription.CloseNotify) {
					if (m_State == HandshakeType.ShuttingDown) { // true if we've already sent a shutdown notification
						// close connection
						ret = new SslHandshakeStatus(SslStatus.Close, null);
					} else {
						// send a shutdown notifications, and then close the connection
						ret = new SslHandshakeStatus(SslStatus.Close, GetControlBytes(ControlType.Shutdown));
					}
				} else {
					ret = new SslHandshakeStatus(SslStatus.OK, null);
				}
				return ret;
			} catch (SslException t) {
				throw t;
			} catch (Exception e) {
				throw new SslException(e, AlertDescription.InternalError, "There was an internal error.");
			}
		}
		protected HandshakeMessage GetHandshakeMessage(byte[] buffer, int offset) {
			if (buffer.Length < offset + 4)
				return null;
			int size = buffer[offset + 1] * 65536 + buffer[offset + 2] * 256 + buffer[offset + 3];
			if (buffer.Length < offset + 4 + size)
				return null;
			byte[] fragment = new byte[size];
			Array.Copy(buffer, offset + 4, fragment, 0, size);
			return new HandshakeMessage((HandshakeType)buffer[offset], fragment);
		}
		protected byte[] GetUnixTime() {
			DateTime now = DateTime.Now.ToUniversalTime();
			TimeSpan time = now.Subtract(new DateTime(1970, 1, 1));
			byte[] ret = BitConverter.GetBytes((uint)time.TotalSeconds);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(ret);
			return ret;
		}
		public bool IsNegotiating() {
			return m_IsNegotiating;
		}
		protected void GenerateCiphers(byte[] premaster) {
			byte[] clientrnd = new byte[32], serverrnd = new byte[32];
			byte[] random = new byte[64];
			Array.Copy(m_ClientTime, 0, clientrnd, 0, 4);
			Array.Copy(m_ClientRandom, 0, clientrnd, 4, 28);
			Array.Copy(m_ServerTime, 0, serverrnd, 0, 4);
			Array.Copy(m_ServerRandom, 0, serverrnd, 4, 28);
			m_MasterSecret = GenerateMasterSecret(premaster, clientrnd, serverrnd);
			m_CipherSuite = CipherSuites.GetCipherSuite(GetProtocol(), m_MasterSecret, clientrnd, serverrnd, m_EncryptionScheme, m_Options.Entity);
			Array.Clear(premaster, 0, premaster.Length);
		}
		protected void UpdateHashes(HandshakeMessage message, HashUpdate update) {
			byte[] header = new byte[4];
			header[0] = (byte)message.type;
			header[1] = (byte)(message.fragment.Length / 65536);
			header[2] = (byte)((message.fragment.Length % 65536) / 256);
			header[3] = (byte)(message.fragment.Length % 256);
			UpdateHashes(header, update);
			UpdateHashes(message.fragment, update);
		}
		protected void UpdateHashes(byte[] buffer, HashUpdate update) {
			if (update == HashUpdate.All || update == HashUpdate.Local || update == HashUpdate.LocalRemote) {
				m_LocalMD5Hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
				m_LocalSHA1Hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
			}
			if (update == HashUpdate.All || update == HashUpdate.Remote || update == HashUpdate.LocalRemote) {
				m_RemoteMD5Hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
				m_RemoteSHA1Hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
			}
			if (update == HashUpdate.All) {
				m_CertSignHash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
			}
		}
		public byte[] GetControlBytes(ControlType type) { // the GetControlBytes only handles Shutdown and Renegotiate; ClientHello should be implemented in inheriting classes
			if (type == ControlType.Shutdown) {
				m_IsNegotiating = true;
				m_State = HandshakeType.ShuttingDown;
				return m_RecordLayer.EncryptBytes(new byte[]{(byte)AlertLevel.Warning, (byte)AlertDescription.CloseNotify}, 0, 2, ContentType.Alert);
			} else if (type == ControlType.Renegotiate){
				return GetRenegotiateBytes();
			} else if (type == ControlType.ClientHello) {
				return GetClientHello();
			} else {
				throw new NotSupportedException("The selected ControlType field is not supported.");
			}
		}
		public SecureSocket Parent {
			get {
				return m_RecordLayer.Parent;
			}
		}
		protected void ClearHandshakeStructures() {
			try {
				m_LocalMD5Hash.Initialize();
				m_LocalSHA1Hash.Initialize();
				m_RemoteMD5Hash.Initialize();
				m_RemoteSHA1Hash.Initialize();
				m_CertSignHash.Initialize();
				if (m_ClientTime != null)
					Array.Clear(m_ClientTime, 0, m_ClientTime.Length);
				if (m_ClientRandom != null)
					Array.Clear(m_ClientRandom, 0, m_ClientRandom.Length);
				if (m_ServerTime != null)
					Array.Clear(m_ServerTime, 0, m_ServerTime.Length);
				if (m_ServerRandom != null)
					Array.Clear(m_ServerRandom, 0, m_ServerRandom.Length);
				if (m_SessionID != null)
					Array.Clear(m_SessionID, 0, m_SessionID.Length);
				if (m_MasterSecret != null)
					Array.Clear(m_MasterSecret, 0, m_MasterSecret.Length);
				if (m_KeyCipher != null)
					m_KeyCipher.Clear();
			} catch {}
		}
		public void Dispose() {
			Dispose(true);
		}
		public void Dispose(bool clear) {
			if (m_Disposed) {
				m_Disposed = true;
				if (clear) {
					ClearHandshakeStructures();
					m_LocalMD5Hash.Clear();
					m_LocalSHA1Hash.Clear();
					m_RemoteMD5Hash.Clear();
					m_RemoteSHA1Hash.Clear();
					m_CertSignHash.Clear();
					if (m_IncompleteMessage != null)
						Array.Clear(m_IncompleteMessage, 0, m_IncompleteMessage.Length);
					if (m_CipherSuite != null) {
						if (m_CipherSuite.Decryptor != null)
							m_CipherSuite.Decryptor.Dispose();
						if (m_CipherSuite.Encryptor != null)
							m_CipherSuite.Encryptor.Dispose();
						if (m_CipherSuite.LocalHasher != null)
							m_CipherSuite.LocalHasher.Clear();
						if (m_CipherSuite.RemoteHasher != null)
							m_CipherSuite.RemoteHasher.Clear();
					}
				}
			}
		}
		~HandshakeLayer() {
			Dispose();
		}
		public RNGCryptoServiceProvider RNG {
			get {
				return m_RNG;
			}
		}
		public SslAlgorithms ActiveEncryption {
			get {
				return m_EncryptionScheme;
			}
		}
		public Certificate RemoteCertificate {
			get {
				return m_RemoteCertificate;
			}
		}
		internal RecordLayer RecordLayer {
			get {
				return m_RecordLayer;
			}
			set {
				m_RecordLayer = value;
			}
		}
		public abstract SecureProtocol GetProtocol();
		public abstract ProtocolVersion GetVersion();
		public abstract SslHandshakeStatus ProcessSsl2Hello(byte[] hello);
		protected abstract SslHandshakeStatus ProcessChangeCipherSpec(RecordMessage message);
		protected abstract SslHandshakeStatus ProcessMessage(HandshakeMessage message);
		protected abstract byte[] GetClientHello();
		protected abstract byte[] GetRenegotiateBytes();
		protected abstract byte[] GenerateMasterSecret(byte[] premaster, byte[] clientRandom, byte[] serverRandom);
		protected abstract byte[] GetFinishedMessage();
		protected abstract void VerifyFinishedMessage(byte[] peerFinished);

		protected SecurityOptions m_Options;
		protected bool m_IsNegotiating;
		protected RNGCryptoServiceProvider m_RNG;
		protected RecordLayer m_RecordLayer;
		protected HandshakeType m_State; // last received message
		protected byte[] m_IncompleteMessage;
		protected byte[] m_ClientTime;
		protected byte[] m_ClientRandom;
		protected byte[] m_ServerTime;
		protected byte[] m_ServerRandom;
		protected byte[] m_SessionID;
		protected byte[] m_MasterSecret;
		protected SslAlgorithms m_CompressionMethod;
		protected SslAlgorithms m_EncryptionScheme;
		protected CipherSuite m_CipherSuite;
		protected Certificate m_RemoteCertificate;
		protected MD5 m_LocalMD5Hash;
		protected SHA1 m_LocalSHA1Hash;
		protected MD5 m_RemoteMD5Hash;
		protected SHA1 m_RemoteSHA1Hash;
		protected MD5SHA1CryptoServiceProvider m_CertSignHash;
		protected bool m_MutualAuthentication;
		protected RSACryptoServiceProvider m_KeyCipher;
		protected bool m_Disposed;
	}
}
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
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Org.Mentalis.Security.Cryptography;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl;

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
	internal abstract class ClientHandshakeLayer : HandshakeLayer {
		public ClientHandshakeLayer(RecordLayer recordLayer, SecurityOptions options) : base(recordLayer, options) {}
		public ClientHandshakeLayer(HandshakeLayer handshakeLayer) : base(handshakeLayer) {}
		protected override SslHandshakeStatus ProcessMessage(HandshakeMessage message) { // throws SslExceptions
			SslHandshakeStatus ret = new SslHandshakeStatus();
			switch(message.type) {
				case HandshakeType.ServerHello:
					ret = ProcessServerHello(message);
					break;
				case HandshakeType.Certificate: // optional
					ret = ProcessCertificate(message, true);
					break;
				case HandshakeType.ServerKeyExchange: // optional
					ret = ProcessServerKeyExchange(message);
					break;
				case HandshakeType.CertificateRequest: // optional
					ret = ProcessCertificateRequest(message);
					break;
				case HandshakeType.ServerHelloDone:
					ret = ProcessServerHelloDone(message);
					break;
				case HandshakeType.Finished:
					ret = ProcessFinished(message);
					break;
				case HandshakeType.HelloRequest:
					ret = ProcessHelloRequest(message);
					break;
				default:
					throw new SslException(AlertDescription.UnexpectedMessage, "The received message was not expected from a server.");
			}
			return ret;
		}
		protected override byte[] GetClientHello() {
			if (m_State != HandshakeType.Nothing && m_State != HandshakeType.Finished)
				throw new SslException(AlertDescription.UnexpectedMessage, "ClientHello message must be the first message or must be preceded by a Finished message.");
			m_IsNegotiating = true;
			m_State = HandshakeType.ClientHello;
			byte[] ciphers = CipherSuites.GetCipherAlgorithmBytes(m_Options.AllowedAlgorithms);
			byte[] compr = CompressionAlgorithm.GetCompressionAlgorithmBytes(m_Options.AllowedAlgorithms);
			HandshakeMessage temp = new HandshakeMessage(HandshakeType.ClientHello, new byte[38 + ciphers.Length + compr.Length]);
			m_ClientTime = GetUnixTime();
			m_ClientRandom = new byte[28];
			m_RNG.GetBytes(m_ClientRandom);
			ProtocolVersion pv = CompatibilityLayer.GetMaxProtocol(m_Options.Protocol);
			temp.fragment[0] = pv.major;
			temp.fragment[1] = pv.minor;
			Array.Copy(m_ClientTime, 0, temp.fragment, 2, 4);
			Array.Copy(m_ClientRandom, 0, temp.fragment, 6, 28);
			temp.fragment[34] = 0; // do not resume a session, and do not let the other side cache it
			temp.fragment[35] = (byte)(ciphers.Length / 256);
			temp.fragment[36] = (byte)(ciphers.Length % 256);
			Array.Copy(ciphers, 0, temp.fragment, 37, ciphers.Length);
			temp.fragment[37 + ciphers.Length] = (byte)compr.Length;
			Array.Copy(compr, 0, temp.fragment, 38 + ciphers.Length, compr.Length);
			byte[] ret = temp.ToBytes();
			UpdateHashes(ret, HashUpdate.All); // client hello message
			return m_RecordLayer.EncryptBytes(ret, 0, ret.Length, ContentType.Handshake);
		}
		protected SslHandshakeStatus ProcessServerHello(HandshakeMessage message) {
			if (m_State != HandshakeType.ClientHello && m_State != HandshakeType.HelloRequest)
				throw new SslException(AlertDescription.UnexpectedMessage, "ServerHello message must be preceded by a ClientHello message.");
			UpdateHashes(message, HashUpdate.All); // input message
			if (message.fragment.Length < 2 || message.fragment[0] != GetVersion().major || message.fragment[1] != GetVersion().minor)
				throw new SslException(AlertDescription.IllegalParameter, "Unknown protocol version of the client.");
			try {
				// extract the time from the client [== 1 uint]
				m_ServerTime = new byte[4];
				Array.Copy(message.fragment, 2, m_ServerTime, 0, 4);
				// extract the random bytes [== 28 bytes]
				m_ServerRandom = new byte[28];
				Array.Copy(message.fragment, 6, m_ServerRandom, 0, 28);
				// extact the session ID [== 0..32 bytes]
				int length = message.fragment[34];
				if (length > 32)
					throw new SslException(AlertDescription.IllegalParameter, "The length of the SessionID cannot be more than 32 bytes.");
				m_SessionID = new byte[length];
				Array.Copy(message.fragment, 35, m_SessionID, 0, length);
				// extract the selected cipher suite
				m_EncryptionScheme = CipherSuites.GetCipherAlgorithmType(message.fragment, 35 + length);
				// extract the selected compression method
				m_CompressionMethod = CompressionAlgorithm.GetCompressionAlgorithmType(message.fragment, 37 + length);
			} catch (Exception e) {
				throw new SslException(e, AlertDescription.InternalError, "The message is invalid.");
			}
			return new SslHandshakeStatus(SslStatus.MessageIncomplete, null);
		}
		protected SslHandshakeStatus ProcessServerKeyExchange(HandshakeMessage message) {
			if (m_State != HandshakeType.Certificate)
				throw new SslException(AlertDescription.UnexpectedMessage, "ServerKeyExchange message must be preceded by a Certificate message.");
			CipherDefinition cd = CipherSuites.GetCipherDefinition(m_EncryptionScheme);
			if (!cd.Exportable)
				throw new SslException(AlertDescription.HandshakeFailure, "The ServerKeyExchange message should not be sent for non-exportable ciphers.");
			if (m_RemoteCertificate.GetPublicKeyLength() <= 512)
				throw new SslException(AlertDescription.HandshakeFailure, "The ServerKeyExchange message should not be sent because the server certificate public key is of exportable length.");
			UpdateHashes(message, HashUpdate.All); // input message
			// extract modulus and exponent
			RSAParameters pars = new RSAParameters();
			int size = message.fragment[0] * 256 + message.fragment[1];
			pars.Modulus = new byte[size];
			Array.Copy(message.fragment, 2, pars.Modulus, 0, size);
			int offset = size + 2;
			size = message.fragment[offset] * 256 + message.fragment[offset + 1];
			pars.Exponent = new byte[size];
			Array.Copy(message.fragment, offset + 2, pars.Exponent, 0, size);
			offset += size + 2;
			pars.Modulus = RemoveLeadingZeros(pars.Modulus);
			pars.Exponent = RemoveLeadingZeros(pars.Exponent);
			m_KeyCipher = new RSACryptoServiceProvider();
			m_KeyCipher.ImportParameters(pars);
			// compute verification hashes
			MD5SHA1CryptoServiceProvider ms = new MD5SHA1CryptoServiceProvider();
			ms.TransformBlock(m_ClientTime, 0, m_ClientTime.Length, m_ClientTime, 0);
			ms.TransformBlock(m_ClientRandom, 0, m_ClientRandom.Length, m_ClientRandom, 0);
			ms.TransformBlock(m_ServerTime, 0, m_ServerTime.Length, m_ServerTime, 0);
			ms.TransformBlock(m_ServerRandom, 0, m_ServerRandom.Length, m_ServerRandom, 0);
			ms.TransformFinalBlock(message.fragment, 0, offset);
			// verify the signature
			size = message.fragment[offset] * 256 + message.fragment[offset + 1];
			byte[] signature = new byte[size]; // holds the signature returned by the server
			Array.Copy(message.fragment, offset + 2, signature, 0, size);
			if (!ms.VerifySignature(m_RemoteCertificate, signature))
				throw new SslException(AlertDescription.HandshakeFailure, "The data was not signed by the server certificate.");
			ms.Clear();
			return new SslHandshakeStatus(SslStatus.MessageIncomplete, null);
		}
		private byte[] RemoveLeadingZeros(byte[] input) {
			int occ = 0;
			for(int i = 0; i < input.Length; i++) {
				if (input[i] != 0)
					break;
				occ++;
			}
			if (occ == 0) {
				return input;
			} else {
				byte[] buffer = new byte[input.Length - occ];
				Array.Copy(input, occ, buffer, 0, buffer.Length);
				return buffer;
			}
		}
		protected SslHandshakeStatus ProcessCertificateRequest(HandshakeMessage message) {
			if (m_State == HandshakeType.ServerKeyExchange) {
				CipherDefinition cd = CipherSuites.GetCipherDefinition(m_EncryptionScheme);
				if (this.m_RemoteCertificate.GetPublicKeyLength() <= 512 || !cd.Exportable)
					throw new SslException(AlertDescription.HandshakeFailure, "Invalid message.");
			} else if (m_State != HandshakeType.Certificate) {
				throw new SslException(AlertDescription.UnexpectedMessage, "CertificateRequest message must be preceded by a Certificate or ServerKeyExchange message.");
			}
			UpdateHashes(message, HashUpdate.All); // input message
			// get supported certificate types
			bool supportsRsaCerts = false;
			byte[] certTypes = new byte[message.fragment[0]]; // currently we're not doing anything with the supported certificate types
			Array.Copy(message.fragment, 1, certTypes, 0, certTypes.Length);
			for(int i = 0; i < certTypes.Length; i++) {
				if (certTypes[i] == 1) { // rsa_sign
					supportsRsaCerts = true;
					break;
				}
			}
			// get list of distinguished names
			if (m_Options.RequestHandler != null && supportsRsaCerts) { // make sure the client passed a delegate
				Queue q = new Queue();
				DistinguishedNameList r = new DistinguishedNameList();
				int size, offset = message.fragment[0] + 3;
				byte[] buffer;
				while(offset < message.fragment.Length) {
					size = message.fragment[offset] * 256 + message.fragment[offset + 1];
					buffer = new byte[size];
					Array.Copy(message.fragment, offset + 2, buffer, 0, size);
					q.Enqueue(buffer);
					offset += size + 2;
				}
				// decode RDN structures
				while(q.Count > 0) {
					r.Add(ProcessName((byte[])q.Dequeue()));
				}
				RequestEventArgs e = new RequestEventArgs();
				try {
					m_Options.RequestHandler(Parent, r, e);
					if (e.Certificate != null)
						m_Options.Certificate = e.Certificate;
				} catch (Exception de) {
					throw new SslException(de, AlertDescription.InternalError, "The code in the CertRequestEventHandler delegate threw an error.");
				}
			}
			if (!supportsRsaCerts)
				m_Options.Certificate = null; // do not send client certificate
			m_MutualAuthentication = true;
			return new SslHandshakeStatus(SslStatus.MessageIncomplete, null);
		}
		private DistinguishedName ProcessName(byte[] buffer) {
			GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try {
				return new DistinguishedName(gch.AddrOfPinnedObject(), buffer.Length);
			} finally {
				gch.Free();
			}
		}
		protected SslHandshakeStatus ProcessServerHelloDone(HandshakeMessage message) {
			if (m_State != HandshakeType.Certificate && m_State != HandshakeType.ServerKeyExchange && m_State != HandshakeType.CertificateRequest)
				throw new SslException(AlertDescription.UnexpectedMessage, "ServerHello message must be preceded by a ClientHello message.");
			if (message.fragment.Length != 0)
				throw new SslException(AlertDescription.IllegalParameter, "The ServerHelloDone message is invalid.");
			UpdateHashes(message, HashUpdate.All); // input message
			MemoryStream ms = new MemoryStream();
			HandshakeMessage hm = new HandshakeMessage(HandshakeType.ClientKeyExchange, null);
			byte[] buffer;
			// send Certificate [optional]
			if (m_MutualAuthentication) {
				hm.type = HandshakeType.Certificate;
				hm.fragment = GetCertificateBytes(m_Options.Certificate);
				buffer = m_RecordLayer.EncryptBytes(hm.ToBytes(), 0, hm.fragment.Length + 4, ContentType.Handshake);
				ms.Write(buffer, 0, buffer.Length);
				UpdateHashes(hm, HashUpdate.All); // output message
			}
			// send ClientKeyExchange
			if (m_KeyCipher == null)
				m_KeyCipher = (RSACryptoServiceProvider)m_RemoteCertificate.PublicKey;
			RSAKeyTransform kf = new RSAKeyTransform(m_KeyCipher);
			byte[] preMasterSecret = new byte[48];
			m_RNG.GetBytes(preMasterSecret);
			ProtocolVersion pv = CompatibilityLayer.GetMaxProtocol(m_Options.Protocol);
			preMasterSecret[0] =  pv.major;
			preMasterSecret[1] = pv.minor;
			buffer = kf.CreateKeyExchange(preMasterSecret); // public-key-encrypt the preMasterSecret
			hm.type = HandshakeType.ClientKeyExchange;
			if (GetProtocol() == SecureProtocol.Ssl3) { // SSL
				hm.fragment = buffer;
			} else { // TLS
				hm.fragment = new byte[buffer.Length + 2];
				Array.Copy(buffer, 0, hm.fragment, 2, buffer.Length);
				hm.fragment[0] = (byte)(buffer.Length / 256); // prepend the length of the preMasterSecret
				hm.fragment[1] = (byte)(buffer.Length % 256);
			}
			GenerateCiphers(preMasterSecret); // generate the local ciphers
			buffer = m_RecordLayer.EncryptBytes(hm.ToBytes(), 0, hm.fragment.Length + 4, ContentType.Handshake);
			ms.Write(buffer, 0, buffer.Length);
			UpdateHashes(hm, HashUpdate.All); // output message
			m_KeyCipher.Clear();
			m_KeyCipher = null;
			// send CertificateVerify [optional]
			if (m_MutualAuthentication && m_Options.Certificate != null) {
				m_CertSignHash.MasterKey = this.m_MasterSecret;
				m_CertSignHash.TransformFinalBlock(buffer, 0, 0); // finalize hash
				buffer = m_CertSignHash.CreateSignature(m_Options.Certificate);
				hm.type = HandshakeType.CertificateVerify;
				hm.fragment = new byte[buffer.Length + 2];
				hm.fragment[0] = (byte)(buffer.Length / 256);
				hm.fragment[1] = (byte)(buffer.Length % 256);
				Array.Copy(buffer, 0, hm.fragment, 2, buffer.Length);
				buffer = m_RecordLayer.EncryptBytes(hm.ToBytes(), 0, hm.fragment.Length + 4, ContentType.Handshake);
				ms.Write(buffer, 0, buffer.Length);
				UpdateHashes(hm, HashUpdate.LocalRemote); // output message
			}
			// send ChangeCipherSpec
			buffer = m_RecordLayer.EncryptBytes(new byte[]{1}, 0, 1, ContentType.ChangeCipherSpec);
			ms.Write(buffer, 0, buffer.Length);
			m_RecordLayer.ChangeLocalState(null, m_CipherSuite.Encryptor, m_CipherSuite.LocalHasher);
			// send Finished
			buffer = GetFinishedMessage();
			UpdateHashes(buffer, HashUpdate.Remote); // output message
			buffer = m_RecordLayer.EncryptBytes(buffer, 0, buffer.Length, ContentType.Handshake);
			ms.Write(buffer, 0, buffer.Length);
			// send empty record [http://www.openssl.org/~bodo/tls-cbc.txt]
			if (this.m_CipherSuite.Encryptor.OutputBlockSize != 1) { // is bulk cipher?
				if (((int)m_Options.Flags & (int)SecurityFlags.DontSendEmptyRecord) == 0) {
					byte[] empty = m_RecordLayer.EncryptBytes(new byte[0], 0, 0, ContentType.ApplicationData);
					ms.Write(empty, 0, empty.Length);
				}
			}
			// finalize
			buffer = ms.ToArray();
			ms.Close();
			return new SslHandshakeStatus(SslStatus.ContinueNeeded, buffer);
		}
		protected byte[] GetCertificateBytes(Certificate certificate) {
			if (certificate == null)
				return new byte[]{0, 0, 0};
			byte[] cert_bytes = certificate.ToCerBuffer();
			byte[] ret = new byte[6 + cert_bytes.Length];
			int size = cert_bytes.Length + 3;
			ret[0] = (byte)(size / 65536);
			ret[1] = (byte)((size % 65536) / 256);
			ret[2] = (byte)(size % 256);
			ret[3] = (byte)(cert_bytes.Length / 65536);
			ret[4] = (byte)((cert_bytes.Length % 65536) / 256);
			ret[5] = (byte)(cert_bytes.Length % 256);
			Array.Copy(cert_bytes, 0, ret, 6, cert_bytes.Length);
			return ret;
		}
		protected SslHandshakeStatus ProcessFinished(HandshakeMessage message) {
			if (m_State != HandshakeType.ChangeCipherSpec)
				throw new SslException(AlertDescription.UnexpectedMessage, "Finished message must be preceded by a ChangeCipherSpec message.");
			// check hash received from client
			this.VerifyFinishedMessage(message.fragment);
			m_IsNegotiating = false;
			ClearHandshakeStructures();
			return new SslHandshakeStatus(SslStatus.OK, null);
		}
		protected SslHandshakeStatus ProcessHelloRequest(HandshakeMessage message) {
			if (IsNegotiating())
				return new SslHandshakeStatus(SslStatus.OK, null); // ignore hello request
			return new SslHandshakeStatus(SslStatus.ContinueNeeded, GetClientHello());
		}
		protected override SslHandshakeStatus ProcessChangeCipherSpec(RecordMessage message) {
			if (message.length != 1 || message.fragment[0] != 1)
				throw new SslException(AlertDescription.IllegalParameter, "The ChangeCipherSpec message was invalid.");
			if (m_State == HandshakeType.ServerHelloDone) {
				m_RecordLayer.ChangeRemoteState(null, m_CipherSuite.Decryptor, m_CipherSuite.RemoteHasher);
				return new SslHandshakeStatus(SslStatus.MessageIncomplete, null); // needs a finished message
			} else {
				throw new SslException(AlertDescription.UnexpectedMessage, "ChangeCipherSpec message must be preceded by a ServerHelloDone message.");
			}
		}
		protected override byte[] GetRenegotiateBytes() {
			if (IsNegotiating())
				return null;
			return GetClientHello();
		}
		public override SslHandshakeStatus ProcessSsl2Hello(byte[] hello) {
			throw new SslException(AlertDescription.InternalError, "This is a client socket; it cannot accept a client hello messages");
		}
	}
}
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
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Org.Mentalis.Security.Cryptography;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Shared;

namespace Org.Mentalis.Security.Ssl.Ssl3 {
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
	internal class Ssl3ServerHandshakeLayer : ServerHandshakeLayer {
		public Ssl3ServerHandshakeLayer(RecordLayer recordLayer, SecurityOptions options) : base(recordLayer, options) {}
		public Ssl3ServerHandshakeLayer(HandshakeLayer handshakeLayer) : base(handshakeLayer) {}
		public override SecureProtocol GetProtocol() {
			return SecureProtocol.Ssl3;
		}
		protected override byte[] GenerateMasterSecret(byte[] premaster, byte[] clientRandom, byte[] serverRandom) {
			return Ssl3CipherSuites.GenerateMasterSecret(premaster, clientRandom, serverRandom);
		}
		protected override byte[] GetFinishedMessage() {
			HandshakeMessage hm = new HandshakeMessage(HandshakeType.Finished, new byte[36]);
			Ssl3HandshakeMac md5 = new Ssl3HandshakeMac(HashType.MD5, m_LocalMD5Hash, m_MasterSecret);
			Ssl3HandshakeMac sha1 = new Ssl3HandshakeMac(HashType.SHA1, m_LocalSHA1Hash, m_MasterSecret);
			md5.TransformFinalBlock(new byte[]{0x53, 0x52, 0x56, 0x52}, 0, 4);
			sha1.TransformFinalBlock(new byte[]{0x53, 0x52, 0x56, 0x52}, 0, 4);
			Array.Copy(md5.Hash, 0, hm.fragment, 0, 16);
			Array.Copy(sha1.Hash, 0, hm.fragment, 16, 20);
			md5.Clear();
			sha1.Clear();
			return hm.ToBytes();
		}
		protected override void VerifyFinishedMessage(byte[] peerFinished) {
			if (peerFinished.Length != 36)
				throw new SslException(AlertDescription.IllegalParameter, "The message is invalid.");
			byte[] hash = new byte[36];
			Ssl3HandshakeMac md5 = new Ssl3HandshakeMac(HashType.MD5, m_RemoteMD5Hash, m_MasterSecret);
			Ssl3HandshakeMac sha1 = new Ssl3HandshakeMac(HashType.SHA1, m_RemoteSHA1Hash, m_MasterSecret);
			md5.TransformFinalBlock(new byte[]{0x43, 0x4C, 0x4E, 0x54}, 0, 4);
			sha1.TransformFinalBlock(new byte[]{0x43, 0x4C, 0x4E, 0x54}, 0, 4);
			Array.Copy(md5.Hash, 0, hash, 0, 16);
			Array.Copy(sha1.Hash, 0, hash, 16, 20);
			for(int i = 0; i < hash.Length; i++) {
				if (hash[i] != peerFinished[i])
					throw new SslException(AlertDescription.HandshakeFailure, "The computed hash verification does not correspond with the one of the client.");
			}
			md5.Clear();
			sha1.Clear();
		}
		public override ProtocolVersion GetVersion() {
			return new ProtocolVersion(3, 0);
		}
	}
}
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
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Ssl3;
using Org.Mentalis.Security.Ssl.Tls1;
using System.Text;
using System.IO;

namespace Org.Mentalis.Security.Ssl.Shared {
	// AES ciphers: http://www.ietf.org/rfc/rfc3268.txt
	// 1024bit export ciphers: http://www.ietf.org/proceedings/99nov/I-D/draft-ietf-tls-56-bit-ciphersuites-00.txt
	internal sealed class CipherSuites {
		private CipherSuites() {}
		public static SslAlgorithms GetCipherAlgorithmType(byte[] buffer, int offset) {
			if (buffer.Length < offset + 2)
				throw new SslException(AlertDescription.InternalError, "Buffer overflow in GetCipherAlgorithm.");
			byte b1 = buffer[offset];
			byte b2 = buffer[offset + 1];
			if (b1 == 0 && b2 == 0)
				return SslAlgorithms.NONE;
			else if(b1 == 0 && b2 == 5)
				return SslAlgorithms.RSA_RC4_128_SHA;
			else if(b1 == 0 && b2 == 4)
				return SslAlgorithms.RSA_RC4_128_MD5;
			else if(b1 == 0 && b2 == 3)
				return SslAlgorithms.RSA_RC4_40_MD5;
			else if(b1 == 0 && b2 == 6)
				return SslAlgorithms.RSA_RC2_40_MD5;
			else if(b1 == 0 && b2 == 9)
				return SslAlgorithms.RSA_DES_56_SHA;
			else if(b1 == 0 && b2 == 10)
				return SslAlgorithms.RSA_3DES_168_SHA;
			else if(b1 == 0 && b2 == 8)
				return SslAlgorithms.RSA_DES_40_SHA;
			else if(b1 == 0 && b2 == 47)
				return SslAlgorithms.RSA_AES_128_SHA;
			else if(b1 == 0 && b2 == 53)
				return SslAlgorithms.RSA_AES_256_SHA;
			else
				return SslAlgorithms.NONE;
		}
		public static byte[] GetCipherAlgorithmBytes(SslAlgorithms algorithm) {
			MemoryStream ms = new MemoryStream();
			// write them to the memory stream in order of preference
			int algo = (int)algorithm;
			if ((algo & (int)SslAlgorithms.RSA_AES_256_SHA) != 0)
				ms.Write(new byte[]{0,53}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_AES_128_SHA) != 0)
				ms.Write(new byte[]{0,47}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_RC4_128_SHA) != 0)
				ms.Write(new byte[]{0,5}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_RC4_128_MD5) != 0)
				ms.Write(new byte[]{0,4}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_3DES_168_SHA) != 0)
				ms.Write(new byte[]{0,10}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_DES_56_SHA) != 0)
				ms.Write(new byte[]{0,9}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_RC4_40_MD5) != 0)
				ms.Write(new byte[]{0,3}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_RC2_40_MD5) != 0)
				ms.Write(new byte[]{0,6}, 0, 2);
			if ((algo & (int)SslAlgorithms.RSA_DES_40_SHA) != 0)
				ms.Write(new byte[]{0,8}, 0, 2);
			return ms.ToArray();
		}
		public static SslAlgorithms GetCipherSuiteAlgorithm(byte[] algorithms, SslAlgorithms allowed) {
			int alwd = (int)allowed;
			for(int i = 0; i < algorithms.Length; i+=2) {
				SslAlgorithms alg = GetCipherAlgorithmType(algorithms, i);
				if (((int)alg & alwd) != 0)
					return alg;
			}
			throw new SslException(AlertDescription.HandshakeFailure, "No encryption scheme matches the available schemes.");
		}
		public static CipherSuite GetCipherSuite(SecureProtocol protocol, byte[] master, byte[] clientrnd, byte[] serverrnd, SslAlgorithms scheme, ConnectionEnd entity) {
			for(int i = 0; i < Definitions.Length; i++) {
				if (Definitions[i].Scheme == scheme) {
					if (protocol == SecureProtocol.Tls1) {
						return Tls1CipherSuites.InitializeCipherSuite(master, clientrnd, serverrnd, Definitions[i], entity);
					} else if (protocol == SecureProtocol.Ssl3) {
						return Ssl3CipherSuites.InitializeCipherSuite(master, clientrnd, serverrnd, Definitions[i], entity); 
					}
				}
			}
			throw new SslException(AlertDescription.IllegalParameter, "The cipher suite is unknown.");
		}
		public static CipherDefinition GetCipherDefinition(SslAlgorithms scheme) {
			for(int i = 0; i < Definitions.Length; i++) {
				if (Definitions[i].Scheme == scheme) {
					return Definitions[i];
				}
			}
			throw new SslException(AlertDescription.IllegalParameter, "The cipher suite is unknown.");
		}
		private static CipherDefinition[] Definitions = new CipherDefinition[] {
				new CipherDefinition(SslAlgorithms.RSA_RC4_128_MD5, typeof(ARCFourManaged), 16, 0, 16, typeof(MD5CryptoServiceProvider), HashType.MD5, 16, false),
				new CipherDefinition(SslAlgorithms.RSA_RC4_128_SHA, typeof(ARCFourManaged), 16, 0, 16, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, false),
				new CipherDefinition(SslAlgorithms.RSA_RC4_40_MD5, typeof(ARCFourManaged), 5, 0, 16, typeof(MD5CryptoServiceProvider), HashType.MD5, 16, true),
				new CipherDefinition(SslAlgorithms.RSA_RC2_40_MD5, typeof(RC2CryptoServiceProvider), 5, 8, 16, typeof(MD5CryptoServiceProvider), HashType.MD5, 16, true),
				new CipherDefinition(SslAlgorithms.RSA_DES_56_SHA, typeof(DESCryptoServiceProvider), 8, 8, 8, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, false),
				new CipherDefinition(SslAlgorithms.RSA_3DES_168_SHA, typeof(TripleDESCryptoServiceProvider), 24, 8, 24, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, false),
				new CipherDefinition(SslAlgorithms.RSA_DES_40_SHA, typeof(DESCryptoServiceProvider), 5, 8, 8, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, true),
				new CipherDefinition(SslAlgorithms.RSA_AES_128_SHA, typeof(RijndaelManaged), 16, 16, 16, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, false),
				new CipherDefinition(SslAlgorithms.RSA_AES_256_SHA, typeof(RijndaelManaged), 32, 16, 32, typeof(SHA1CryptoServiceProvider), HashType.SHA1, 20, false)
			};
	}
	internal class CipherSuite {
		public ICryptoTransform Decryptor;
		public ICryptoTransform Encryptor;
		public KeyedHashAlgorithm LocalHasher;
		public KeyedHashAlgorithm RemoteHasher;
	}
	internal struct CipherDefinition {
		public CipherDefinition(SslAlgorithms scheme, Type bulk, int keysize, int ivsize, int expsize, Type hash, HashType hashType, int hashsize, bool exportable) {
			this.Scheme = scheme;
			this.BulkCipherAlgorithm = bulk;
			this.BulkKeySize = keysize;
			this.BulkIVSize = ivsize;
			this.BulkExpandedSize = expsize;
			this.HashAlgorithm = hash;
			this.HashAlgorithmType = hashType;
			this.HashSize = hashsize;
			this.Exportable = exportable;
		}
		public SslAlgorithms Scheme;
		public Type BulkCipherAlgorithm;
		public int BulkKeySize; // in bytes
		public int BulkIVSize; // in bytes
		public int BulkExpandedSize; // in bytes
		public Type HashAlgorithm;
		public int HashSize; // in bytes
		public bool Exportable;
		public HashType HashAlgorithmType;
	}
}
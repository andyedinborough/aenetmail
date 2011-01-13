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
using System.Runtime.InteropServices;
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl.Ssl3;

namespace Org.Mentalis.Security.Ssl.Shared {
	internal sealed class MD5SHA1CryptoServiceProvider : HashAlgorithm {
		public MD5SHA1CryptoServiceProvider() {
			this.HashSizeValue = 36;
			m_MD5 = new MD5CryptoServiceProvider();
			m_SHA1 = new SHA1CryptoServiceProvider();
		}
		protected override void Dispose(bool disposing) {
			m_MD5.Clear();
			m_SHA1.Clear();
			if (m_MasterKey != null)
				Array.Clear(m_MasterKey, 0, m_MasterKey.Length);
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		public override void Initialize() {
			m_MD5.Initialize();
			m_SHA1.Initialize();
		}
		protected override void HashCore(byte[] array, int ibStart, int cbSize) {
			m_MD5.TransformBlock(array, ibStart, cbSize, array, ibStart);
			m_SHA1.TransformBlock(array, ibStart, cbSize, array, ibStart);
		}
		public SecureProtocol Protocol {
			get {
				return m_Protocol;
			}
			set {
				m_Protocol = value;
			}
		}
		public byte[] MasterKey {
			get {
				return m_MasterKey;
			}
			set {
				m_MasterKey = (byte[])value.Clone();
			}
		}
		protected override byte[] HashFinal() {
			if (m_Protocol == SecureProtocol.Ssl3) {
				m_MD5 = new Ssl3HandshakeMac(HashType.MD5, m_MD5, m_MasterKey);
				m_SHA1 = new Ssl3HandshakeMac(HashType.SHA1, m_SHA1, m_MasterKey);
			}
			byte[] hash = new byte[36];
			m_MD5.TransformFinalBlock(hash, 0, 0);
			m_SHA1.TransformFinalBlock(hash, 0, 0);
			Array.Copy(m_MD5.Hash, 0, hash, 0, 16);
			Array.Copy(m_SHA1.Hash, 0, hash, 16, 20);
			return hash;
		}
		public bool VerifySignature(Certificate cert, byte[] signature) {
			return VerifySignature(cert, signature, this.Hash);
		}
		public bool VerifySignature(Certificate cert, byte[] signature, byte[] hash) {
			int provider = 0;
			int hashptr = 0;
			int pubKey = 0;
			try {
				if (SspiProvider.CryptAcquireContext(ref provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, 0) == 0) {
					if (Marshal.GetLastWin32Error() == SecurityConstants.NTE_BAD_KEYSET)
						SspiProvider.CryptAcquireContext(ref provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, SecurityConstants.CRYPT_NEWKEYSET);
				}
				if (provider == 0)
					throw new CryptographicException("Unable to acquire a cryptographic context.");
				if (SspiProvider.CryptCreateHash(provider, SecurityConstants.CALG_SSL3_SHAMD5, 0, 0, out hashptr) == 0)
					throw new CryptographicException("Unable to create the SHA-MD5 hash.");
				if (SspiProvider.CryptSetHashParam(hashptr, SecurityConstants.HP_HASHVAL, hash, 0) == 0)
					throw new CryptographicException("Unable to set the value of the SHA-MD5 hash.");
				CertificateInfo ci = cert.GetCertificateInfo();
				CERT_PUBLIC_KEY_INFO pki = new CERT_PUBLIC_KEY_INFO(ci);
				if (SspiProvider.CryptImportPublicKeyInfo(provider, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, ref pki, out pubKey) == 0)
					throw new CryptographicException("Unable to get a handle to the public key of the specified certificate.");
				byte[] sign_rev = new byte[signature.Length];
				Array.Copy(signature, 0, sign_rev, 0, signature.Length);
				Array.Reverse(sign_rev);
				return SspiProvider.CryptVerifySignature(hashptr, sign_rev, sign_rev.Length, pubKey, IntPtr.Zero, 0) != 0;
			} finally {
				if (pubKey != 0)
					SspiProvider.CryptDestroyKey(pubKey);
				if (hashptr != 0)
					SspiProvider.CryptDestroyHash(hashptr);
				if (provider != 0)
					SspiProvider.CryptReleaseContext(provider, 0);
			}
		}
		public byte[] CreateSignature(Certificate cert) {
			return CreateSignature(cert, this.Hash);
		}
		public byte[] CreateSignature(Certificate cert, byte[] hash) {
			int flags = 0, mustFree = 0, provider = 0, keySpec = 0, hashptr = 0, size = 0;
			try {
				if (!Environment.UserInteractive)
					flags = SecurityConstants.CRYPT_ACQUIRE_SILENT_FLAG;
				if (SspiProvider.CryptAcquireCertificatePrivateKey(cert.Handle, flags, IntPtr.Zero, ref provider, ref keySpec, ref mustFree) == 0)
					throw new SslException(AlertDescription.InternalError, "Could not acquire private key.");
				if (SspiProvider.CryptCreateHash(provider, SecurityConstants.CALG_SSL3_SHAMD5, 0, 0, out hashptr) == 0)
					throw new CryptographicException("Unable to create the SHA-MD5 hash.");
				if (SspiProvider.CryptSetHashParam(hashptr, SecurityConstants.HP_HASHVAL, hash, 0) == 0)
					throw new CryptographicException("Unable to set the value of the SHA-MD5 hash.");
				SspiProvider.CryptSignHash(hashptr, keySpec, IntPtr.Zero, 0, null, ref size);
				if (size == 0)
					throw new CryptographicException("Unable to sign the data.");
				byte[] buffer = new byte[size];
				if (SspiProvider.CryptSignHash(hashptr, keySpec, IntPtr.Zero, 0, buffer, ref size) == 0)
					throw new CryptographicException("Unable to sign the data.");
				Array.Reverse(buffer);
				return buffer;
			} finally {
				if (hashptr != 0)
					SspiProvider.CryptDestroyHash(hashptr);
				if (mustFree != 0 && provider != 0)
					SspiProvider.CryptReleaseContext(provider, 0);
			}
		}
		~MD5SHA1CryptoServiceProvider() {
			Clear();
		}
		private HashAlgorithm m_MD5;
		private HashAlgorithm m_SHA1;
		private SecureProtocol m_Protocol;
		private byte[] m_MasterKey;
	}
}

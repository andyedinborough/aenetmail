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
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Ssl.Tls1 {
/*  PRF(secret, label, seed) = P_MD5(S1, label + seed) XOR
	P_SHA-1(S2, label + seed); */
	internal class PseudoRandomDeriveBytes : DeriveBytes,IDisposable {
		public PseudoRandomDeriveBytes(byte[] secret, string label, byte[] seed) {
			if (label == null)
				throw new ArgumentNullException();
			Initialize(secret, Encoding.ASCII.GetBytes(label), seed);
		}
		public PseudoRandomDeriveBytes(byte[] secret, byte[] label, byte[] seed) {
			if (label == null)
				throw new ArgumentNullException();
			Initialize(secret, label, seed);
		}
		protected void Initialize(byte[] secret, byte[] label, byte[] seed) {
			if (secret == null || seed == null)
				throw new ArgumentNullException();
			m_Disposed = false;
			// ls = label + seed
			byte[] ls = new byte[label.Length + seed.Length];
			Array.Copy(label, 0, ls, 0, label.Length);
			Array.Copy(seed, 0, ls, label.Length, seed.Length);
			// split the secret in two halves
			int length;
			if (secret.Length % 2 == 0) {
				length = secret.Length / 2;
			} else {
				length = (secret.Length / 2) + 1;
			}
			byte[] s1 = new byte[length];
			byte[] s2 = new byte[length];
			Array.Copy(secret, 0, s1, 0, length);
			Array.Copy(secret, secret.Length - length, s2, 0, length);
			// create ExpansionDeriveBytes objects
			m_MD5 = new ExpansionDeriveBytes(new MD5CryptoServiceProvider(), s1, ls);
			m_SHA1 = new ExpansionDeriveBytes(new SHA1CryptoServiceProvider(), s2, ls);
		}
		public override byte[] GetBytes(int cb) { // get the next bytes
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			byte[] md5 = m_MD5.GetBytes(cb);
			byte[] sha1 = m_SHA1.GetBytes(cb);
			byte[] ret = new byte[cb];
			for(int i = 0; i < ret.Length; i++) {
				ret[i] = (byte)(md5[i] ^ sha1[i]);
			}
			return ret;
		}
		public override void Reset() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_MD5.Reset();
			m_SHA1.Reset();
		}
		public void Dispose() {
			if (!m_Disposed) {
				m_Disposed = true;
				m_MD5.Dispose();
				m_SHA1.Dispose();
			}
		}
		~PseudoRandomDeriveBytes() {
			Dispose();
		}
		private ExpansionDeriveBytes m_MD5;
		private ExpansionDeriveBytes m_SHA1;
		private bool m_Disposed;
	}
}
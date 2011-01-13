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
	/* P_hash(secret, seed) = HMAC_hash(secret, A(1) + seed) +
								  HMAC_hash(secret, A(2) + seed) +
								  HMAC_hash(secret, A(3) + seed) + ...
	   Where + indicates concatenation.
	   A() is defined as:
		   A(0) = seed
		   A(i) = HMAC_hash(secret, A(i-1))
	 */
	internal class ExpansionDeriveBytes : DeriveBytes, IDisposable {
		public ExpansionDeriveBytes(HashAlgorithm hash, byte[] secret, string seed) {
			if (seed == null)
				throw new ArgumentNullException();
			Initialize(hash, secret, Encoding.ASCII.GetBytes(seed));
		}
		public ExpansionDeriveBytes(HashAlgorithm hash, byte[] secret, byte[] seed) {
			Initialize(hash, secret, seed);
		}
		protected void Initialize(HashAlgorithm hash, byte[] secret, byte[] seed) {
			if (seed == null || secret == null || hash == null)
				throw new ArgumentNullException();
			m_Disposed = false;
            m_HMAC = new Org.Mentalis.Security.Cryptography.HMAC(hash, secret);
			m_Seed = seed;
			m_HashSize = m_HMAC.HashSize / 8;
			Reset();
		}
		protected byte[] GetNextBytes() {
			m_HMAC.TransformBlock(m_Ai, 0, m_HashSize, m_Ai, 0);
			m_HMAC.TransformFinalBlock(m_Seed, 0, m_Seed.Length);
			byte[] ret = m_HMAC.Hash;
			m_HMAC.Initialize();
			// calculate next A
			m_Ai = m_HMAC.ComputeHash(m_Ai);
			return ret;
		}
		public override byte[] GetBytes(int cb) { // get the next bytes
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (cb < 0)
				throw new ArgumentException();
			byte[] ret = new byte[cb];
			byte[] temp;
			int filled = 0;
			while(filled < ret.Length) {
				if (filled + m_NextBytes.Length >= cb) {
					Array.Copy(m_NextBytes, 0, ret, filled, cb - filled);
					temp = new byte[m_NextBytes.Length - (cb - filled)];
					Array.Copy(m_NextBytes, m_NextBytes.Length - temp.Length, temp, 0, temp.Length);
					m_NextBytes = temp;
					filled = ret.Length;
				} else {
					Array.Copy(m_NextBytes, 0, ret, filled, m_NextBytes.Length);
					filled += m_NextBytes.Length;
					m_NextBytes = GetNextBytes();
				}
			}
			return ret;
		}
		public override void Reset() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_Ai = m_HMAC.ComputeHash(m_Seed); // A(1)
			m_NextBytes = GetNextBytes();
		}
		public void Dispose() {
			if (!m_Disposed) {
				m_Disposed = true;
				m_HMAC.Clear();
				Array.Clear(m_Seed, 0, m_Seed.Length);
				Array.Clear(m_Ai, 0, m_Ai.Length);
				Array.Clear(m_NextBytes, 0, m_NextBytes.Length);
			}
		}
		~ExpansionDeriveBytes() {
			Dispose();
		}
		private Org.Mentalis.Security.Cryptography.HMAC m_HMAC;
		private int m_HashSize; // in bytes
		private byte[] m_Seed;
		private byte[] m_NextBytes;
		private byte[] m_Ai;
		private bool m_Disposed;
	}
}
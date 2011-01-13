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

namespace Org.Mentalis.Security.Ssl.Ssl3 {
	/* master_secret =
		MD5(pre_master_secret + SHA('A' + pre_master_secret +
			ClientHello.random + ServerHello.random)) +
		MD5(pre_master_secret + SHA('BB' + pre_master_secret +
			ClientHello.random + ServerHello.random)) +
		MD5(pre_master_secret + SHA('CCC' + pre_master_secret +
			ClientHello.random + ServerHello.random));
	 */
	internal class Ssl3DeriveBytes : DeriveBytes,IDisposable {
		//clientServer: true if random bytes should be processed as first the client bytes, then the server bytes
		//              false otherwise
		public Ssl3DeriveBytes(byte[] secret, byte[] clientRandom, byte[] serverRandom, bool clientServer) {
			if (secret == null || clientRandom == null || serverRandom == null)
				throw new ArgumentNullException();
			if (clientRandom.Length != 32 || serverRandom.Length != 32)
				throw new ArgumentException();
			m_Disposed = false;
			m_Secret = (byte[])secret.Clone();
			m_Random = new byte[64];
			if (clientServer) {
				Array.Copy(clientRandom, 0, m_Random, 0, 32);
				Array.Copy(serverRandom, 0, m_Random, 32, 32);
			} else {
				Array.Copy(serverRandom, 0, m_Random, 0, 32);
				Array.Copy(clientRandom, 0, m_Random, 32, 32);
			}
			m_MD5 = new MD5CryptoServiceProvider();
			m_SHA1 = new SHA1CryptoServiceProvider();
			Reset();
		}
		protected byte[] GetNextBytes() {
			if (m_Iteration > 26)
				throw new CryptographicException("The SSL3 pseudo random function can only output 416 bytes.");
			byte[] ret = new byte[m_Iteration];
			for(int i = 0; i < ret.Length; i++) {
				ret[i] = (byte)(64 + m_Iteration);
			}
			m_SHA1.TransformBlock(ret, 0, ret.Length, ret, 0);
			m_SHA1.TransformBlock(m_Secret, 0, m_Secret.Length, m_Secret, 0);
			m_SHA1.TransformFinalBlock(m_Random, 0, m_Random.Length);
			m_MD5.TransformBlock(m_Secret, 0, m_Secret.Length, m_Secret, 0);
			m_MD5.TransformFinalBlock(m_SHA1.Hash, 0, 20);
			// finalize
			ret = m_MD5.Hash;
			m_SHA1.Initialize();
			m_MD5.Initialize();
			m_Iteration++;
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
			m_Iteration = 1;
			m_NextBytes = GetNextBytes();
		}
		public void Dispose() {
			if (!m_Disposed) {
				m_Disposed = true;
				m_MD5.Clear();
				m_SHA1.Clear();
				Array.Clear(m_Secret, 0, m_Secret.Length);
				Array.Clear(m_NextBytes, 0, m_NextBytes.Length);
				Array.Clear(m_Random, 0, m_Random.Length);
			}
		}
		~Ssl3DeriveBytes() {
			Dispose();
		}
		private byte[] m_Secret;
		private byte[] m_NextBytes;
		private byte[] m_Random;
		private bool m_Disposed;
		private MD5 m_MD5;
		private SHA1 m_SHA1;
		private int m_Iteration;
	}
}
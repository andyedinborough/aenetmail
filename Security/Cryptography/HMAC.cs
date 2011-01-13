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
using Org.Mentalis.Security;

namespace Org.Mentalis.Security.Cryptography {
	// http://www.ietf.org/rfc/rfc2104.txt
	/// <summary>
	/// Implements the HMAC keyed message authentication code algorithm.
	/// </summary>
	public sealed class HMAC : KeyedHashAlgorithm {
		/// <summary>
		/// Initializes a new instance of the <see cref="HMAC"/> class. This class cannot be inherited.
		/// </summary>
		/// <param name="hash">The underlying hash algorithm to use.</param>
		/// <remarks>A random key will be generated and used by the HMAC.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="hash"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public HMAC(HashAlgorithm hash) : this(hash, null) {}
		/// <summary>
		/// Initializes a new instance of the <see cref="HMAC"/> class.
		/// </summary>
		/// <param name="hash">The underlying hash algorithm to use.</param>
		/// <param name="rgbKey">The key to use for the HMAC -or- a null reference (<b>Nothing</b> in Visual Basic).</param>
		/// <remarks>If <paramref name="rgbKey"/> is a null reference, the HMAC class will generate a random key.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="hash"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public HMAC(HashAlgorithm hash, byte[] rgbKey) {
			if (hash == null)
				throw new ArgumentNullException();
			if (rgbKey == null) {
				rgbKey = new byte[hash.HashSize / 8];
				new RNGCryptoServiceProvider().GetBytes(rgbKey);
			}
			m_HashAlgorithm = hash;
			this.Key = (byte[])rgbKey.Clone();
			m_IsDisposed = false;
			m_KeyBuffer = new byte[64];
			m_Padded = new byte[64];
			Initialize();
		}
		/// <summary>
		/// Initializes the HMAC.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The HMAC instance has been disposed.</exception>
		public override void Initialize() {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_HashAlgorithm.Initialize();
			m_IsHashing = false;
			this.State = 0;
			Array.Clear(m_KeyBuffer, 0, m_KeyBuffer.Length);
		}
		/// <summary>
		/// Routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="rgb">The input for which to compute the hash code. </param>
		/// <param name="ib">The offset into the byte array from which to begin using data. </param>
		/// <param name="cb">The number of bytes in the byte array to use as data. </param>
		/// <exception cref="ObjectDisposedException">The HMAC instance has been disposed.</exception>
		protected override void HashCore(byte[] rgb, int ib, int cb) {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (!m_IsHashing) {
				byte[] key;
				if (this.Key.Length > 64)
					key = m_HashAlgorithm.ComputeHash(this.Key);
				else
					key = this.Key;
				Array.Copy(key, 0, m_KeyBuffer, 0, key.Length);
				for(int i = 0; i < 64; i++)
					m_Padded[i] = (byte)(m_KeyBuffer[i] ^ 0x36);
				m_HashAlgorithm.TransformBlock(m_Padded, 0, m_Padded.Length, m_Padded, 0);
				m_IsHashing = true;
			}
			m_HashAlgorithm.TransformBlock(rgb, ib, cb, rgb, ib);
		}
		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>The computed hash code.</returns>
		/// <exception cref="ObjectDisposedException">The HMAC instance has been disposed.</exception>
		protected override byte[] HashFinal() {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
			byte[] dataHash = m_HashAlgorithm.Hash;
			for(int i = 0; i < 64; i++)
				m_Padded[i] = (byte)(m_KeyBuffer[i] ^ 0x5C);
			m_HashAlgorithm.Initialize();
			m_HashAlgorithm.TransformBlock(m_Padded, 0, m_Padded.Length, m_Padded, 0);
			m_HashAlgorithm.TransformFinalBlock(dataHash, 0, dataHash.Length);
			dataHash = m_HashAlgorithm.Hash;
			Array.Clear(m_KeyBuffer, 0, m_KeyBuffer.Length);
			m_IsHashing = false;
			return dataHash;
		}
		/// <summary>
		/// Gets the size of the computed hash code in bits.
		/// </summary>
		/// <value>The size of the computed hash code in bits.</value>
		public override int HashSize {
			get {
				return m_HashAlgorithm.HashSize;
			}
		}
		/// <summary>
		/// Releases the resources used by the HMAC.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			m_IsDisposed = true;
			base.Dispose(true);
			m_HashAlgorithm.Clear();
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		/// <summary>
		/// Finalizes the HMAC.
		/// </summary>
		~HMAC() {
			m_HashAlgorithm.Clear();
		}
		/// <summary>
		/// Holds the internal hash algorithm
		/// </summary>
		private HashAlgorithm m_HashAlgorithm;
		/// <summary>
		/// Holds the key buffer.
		/// </summary>
		private byte[] m_KeyBuffer;
		/// <summary>
		/// <b>true</b> if a hash operation is in prograss, <b>false</b> otherwise.
		/// </summary>
		private bool m_IsHashing;
		/// <summary>
		/// <b>true</b> if the object has been disposed, <b>false</b> otherwise.
		/// </summary>
		private bool m_IsDisposed;
		private byte[] m_Padded;
	}
}
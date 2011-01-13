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
	/// <summary>
	/// Computes the <see cref="MD4"/> hash for the input data using the implementation provided by the cryptographic service provider (CSP).
	/// </summary>
	/// <remarks>Warning: The MD4 algorithm is a broken algorithm. It should <i>only</i> be used for compatibility with older systems.</remarks>
	public sealed class MD4CryptoServiceProvider : MD4 {
		/// <summary>
		/// Initializes a new instance of the <see cref="MD4CryptoServiceProvider"/> class. This class cannot be inherited.
		/// </summary>
		public MD4CryptoServiceProvider() {
			// acquire an MD4 context
			m_Provider = CAPIProvider.Handle;
/*			if (SspiProvider.CryptAcquireContext(ref m_Provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, 0) == 0) {
				if (Marshal.GetLastWin32Error() == SecurityConstants.NTE_BAD_KEYSET)
					SspiProvider.CryptAcquireContext(ref m_Provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, SecurityConstants.CRYPT_NEWKEYSET);
			}*/
			Initialize();
			m_Disposed = false;
		}
		/// <summary>
		/// Initializes an instance of <see cref="MD4CryptoServiceProvider"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The MD4CryptoServiceProvider instance has been disposed.</exception>
		public override void Initialize() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (m_Hash != 0) {
				SspiProvider.CryptDestroyHash(m_Hash);
			}
			SspiProvider.CryptCreateHash(m_Provider, SecurityConstants.CALG_MD4, 0, 0, out m_Hash);
		}
		/// <summary>
		/// Routes data written to the object into the <see cref="MD4"/> hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The array of data bytes.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the array to use as data.</param>
		/// <exception cref="ObjectDisposedException">The MD4CryptoServiceProvider instance has been disposed.</exception>
		/// <exception cref="CryptographicException">The data could not be hashed.</exception>
		protected override void HashCore(byte[] array, int ibStart, int cbSize) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			byte[] copy = new byte[cbSize];
			Array.Copy(array, ibStart, copy, 0, cbSize);
			if (SspiProvider.CryptHashData(m_Hash, copy, copy.Length, 0) == 0)
				throw new CryptographicException("The data could not be hashed.");
		}
		/// <summary>
		/// Returns the computed <see cref="MD4CryptoServiceProvider"/> hash as an array of bytes after all data has been written to the object.
		/// </summary>
		/// <returns>The computed hash value.</returns>
		/// <exception cref="ObjectDisposedException">The MD4CryptoServiceProvider instance has been disposed.</exception>
		/// <exception cref="CryptographicException">The data could not be hashed.</exception>
		protected override byte[] HashFinal() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			byte[] buffer = new byte[16];
			int length = buffer.Length;
			if (SspiProvider.CryptGetHashParam(m_Hash, SecurityConstants.HP_HASHVAL, buffer, ref length, 0) == 0)
				throw new CryptographicException("The hash value could not be read.");
			return buffer;
		}
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="MD4CryptoServiceProvider"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (!m_Disposed) {
				if (m_Hash != 0) {
					SspiProvider.CryptDestroyHash(m_Hash);
					m_Hash = 0;
				}
/*				if (m_Provider != 0) {
					SspiProvider.CryptReleaseContext(m_Provider, 0);
					m_Provider = 0;
				}*/
				try {
					GC.SuppressFinalize(this);
				} catch {}
				m_Disposed = true;
			}
		}
		/// <summary>
		/// Finalizes the MD4CryptoServiceProvider.
		/// </summary>
		~MD4CryptoServiceProvider() {
			Clear();
		}
		/// <summary>
		/// Holds the handle of the crypto service provider.
		/// </summary>
		private int m_Provider;
		/// <summary>
		/// Holds the hash handle.
		/// </summary>
		private int m_Hash;
		/// <summary>
		/// <b>true</b> if the object ahs been disposed, <b>false</b> otherwise.
		/// </summary>
		private bool m_Disposed;
	}
}
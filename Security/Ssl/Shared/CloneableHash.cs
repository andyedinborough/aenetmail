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
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Ssl.Shared {
	internal sealed class CloneableHash : HashAlgorithm, ICloneable {
		public CloneableHash(HashType type) {
			m_Type = type;
			m_Provider = CAPIProvider.Handle;
			Initialize();
			m_Disposed = false;
		}
		public CloneableHash(int hash, HashType type, int size) {
			m_Provider = CAPIProvider.Handle;
			m_Type = type;
			m_Size = size;
			m_Disposed = false;
			if (SspiProvider.CryptDuplicateHash(hash, IntPtr.Zero, 0, out  m_Hash) == 0)
				throw new CryptographicException("Couldn't duplicate hash.");
		}
		public override void Initialize() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (m_Hash != 0) {
				SspiProvider.CryptDestroyHash(m_Hash);
			}
			int type = SecurityConstants.CALG_SHA1;
			m_Size = 20;
			if (m_Type == HashType.MD5) {
				type = SecurityConstants.CALG_MD5;
				m_Size = 16;
			}
			SspiProvider.CryptCreateHash(m_Provider, type, 0, 0, out m_Hash);
		}
		protected override void HashCore(byte[] array, int ibStart, int cbSize) {
			if (ibStart > 0) {
				GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
				try {
					IntPtr address = handle.AddrOfPinnedObject();
					if (SspiProvider.CryptHashData(m_Hash, new IntPtr(address.ToInt64() + ibStart), cbSize, 0) == 0)
						throw new CryptographicException("The data could not be hashed.");
				} finally {
					handle.Free();
				}
			} else {
				if (SspiProvider.CryptHashData(m_Hash, array, cbSize, 0) == 0)
					throw new CryptographicException("The data could not be hashed.");
			}
		}
		protected override byte[] HashFinal() {
			byte[] buffer = new byte[m_Size];
			int length = buffer.Length;
			if (SspiProvider.CryptGetHashParam(m_Hash, SecurityConstants.HP_HASHVAL, buffer, ref length, 0) == 0)
				throw new CryptographicException("The hash value could not be read.");
			return buffer;
		}
		public object Clone() {
			return new CloneableHash(this.m_Hash, this.m_Type, this.m_Size);
		}
		protected override void Dispose(bool disposing) {
			if (!m_Disposed) {
				if (m_Hash != 0) {
					SspiProvider.CryptDestroyHash(m_Hash);
					m_Hash = 0;
				}
				try {
					GC.SuppressFinalize(this);
				} catch {}
				m_Disposed = true;
			}
		}
		~CloneableHash() {
			Clear();
		}
		private int m_Provider;
		private int m_Hash;
		private bool m_Disposed;
		private HashType m_Type;
		private int m_Size;
	}
}
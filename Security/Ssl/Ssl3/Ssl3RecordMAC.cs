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

namespace Org.Mentalis.Security.Ssl.Ssl3 {
	internal sealed class Ssl3RecordMAC : KeyedHashAlgorithm {
//		hash(MAC_write_secret + pad_2 +
//			hash (MAC_write_secret + pad_1 + seq_num + length + content));
		public Ssl3RecordMAC(HashType hash) : this(hash, null) {}
		public Ssl3RecordMAC(HashType hash, byte[] rgbKey) {
			if (rgbKey == null)
				throw new ArgumentNullException();
			if (hash == HashType.MD5) {
				m_HashAlgorithm = new MD5CryptoServiceProvider();
				m_PadSize = 48;
			} else { // SHA1
				m_HashAlgorithm = new SHA1CryptoServiceProvider();
				m_PadSize = 40;
			}
			KeyValue = (byte[])rgbKey.Clone();
			m_IsDisposed = false;
			Initialize();
		}
		public override void Initialize() {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_HashAlgorithm.Initialize();
			m_IsHashing = false;
			this.State = 0;
		}
		protected override void HashCore(byte[] rgb, int ib, int cb) {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (!m_IsHashing) {
				m_HashAlgorithm.TransformBlock(this.KeyValue, 0, this.KeyValue.Length, this.KeyValue, 0);
				byte[] padding = new byte[m_PadSize];
				for(int i = 0; i < padding.Length; i++)
					padding[i] = 0x36;
				m_HashAlgorithm.TransformBlock(padding, 0, padding.Length, padding, 0);
				m_IsHashing = true;
			}
			m_HashAlgorithm.TransformBlock(rgb, ib, cb, rgb, ib);
		}
		protected override byte[] HashFinal() {
			if (m_IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0); // finalize inner hash
			byte[] dataHash = m_HashAlgorithm.Hash;
			byte[] padding = new byte[m_PadSize];
			for(int i = 0; i < padding.Length; i++)
				padding[i] = 0x5C;
			m_HashAlgorithm.Initialize();
			m_HashAlgorithm.TransformBlock(KeyValue, 0, KeyValue.Length, KeyValue, 0);
			m_HashAlgorithm.TransformBlock(padding, 0, padding.Length, padding, 0);
			m_HashAlgorithm.TransformFinalBlock(dataHash, 0, dataHash.Length);
			m_IsHashing = false; // allow key change
			return m_HashAlgorithm.Hash;
		}
		public override int HashSize {
			get {
				return m_HashAlgorithm.HashSize;
			}
		}
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			m_IsDisposed = true;
			m_HashAlgorithm.Clear();
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		~Ssl3RecordMAC() {
			m_HashAlgorithm.Clear();
		}
		private HashAlgorithm m_HashAlgorithm;
		private bool m_IsHashing;
		private bool m_IsDisposed;
		private int m_PadSize;
	}
}
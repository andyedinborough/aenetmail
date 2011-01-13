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
	/// Computes the <see cref="RIPEMD160"/> hash for the input data.
	/// </summary>
	/// <remarks>Based on the papers located at <a href="http://www.esat.kuleuven.ac.be/~cosicart/ps/AB-9601/">the RIPEMD homepage</a>.</remarks>
	public sealed class RIPEMD160Managed : RIPEMD160 {
		/// <summary>
		/// Initializes a new instance of the <see cref="RIPEMD160Managed"/> class. This class cannot be inherited.
		/// </summary>
		public RIPEMD160Managed() {
			m_X = new uint[16];
			m_HashValue = new uint[5];
			m_ExtraData = new byte[0];
			m_Disposed = false;
			Initialize();
		}
		/// <summary>
		/// When overridden in a derived class, gets the input block size.
		/// </summary>
		/// <value>The input block size.</value>
		public override int InputBlockSize {
			get {
				return 64;
			}
		}
		/// <summary>
		/// Initializes an instance of <see cref="RIPEMD160Managed"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The RIPEMD160Managed instance has been disposed.</exception>
		public override void Initialize() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			m_HashValue[0] = 0x67452301;
			m_HashValue[1] = 0xefcdab89;
			m_HashValue[2] = 0x98badcfe;
			m_HashValue[3] = 0x10325476;
			m_HashValue[4] = 0xc3d2e1f0;
			m_ExtraData = new byte[0];
			m_Length = 0;
		}
		/// <summary>
		/// Routes data written to the object into the <see cref="RIPEMD160"/> hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The array of data bytes.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the array to use as data.</param>
		/// <exception cref="ObjectDisposedException">The <see cref="RIPEMD160Managed"/> instance has been disposed.</exception>
		protected override void HashCore(byte[] array, int ibStart, int cbSize) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (cbSize == 0)
				return;
			int offset = 0;
			byte[] total = new byte[m_ExtraData.Length + cbSize];
			Array.Copy(m_ExtraData, 0, total, 0, m_ExtraData.Length);
			Array.Copy(array, ibStart, total, m_ExtraData.Length, cbSize);
			GCHandle gch = GCHandle.Alloc(m_X, GCHandleType.Pinned);
			IntPtr pointer = gch.AddrOfPinnedObject();
			while(total.Length - offset >= 64) { // blocks must be handled in 64-byte blocks [array of 16 uints]
				Marshal.Copy(total, offset, pointer, 64);
				Compress();
				offset += 64;
			}
			gch.Free();
			m_ExtraData = new byte[total.Length - offset];
			Array.Copy(total, offset, m_ExtraData, 0, m_ExtraData.Length);
			Array.Clear(total, 0, total.Length);
			m_Length += (uint)cbSize;
		}
		/// <summary>
		/// Returns the computed <see cref="RIPEMD160"/> hash as an array of bytes after all data has been written to the object.
		/// </summary>
		/// <returns>The computed hash value.</returns>
		/// <exception cref="ObjectDisposedException">The <see cref="RIPEMD160Managed"/> instance has been disposed.</exception>
		protected override byte[] HashFinal() {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			CompressFinal(m_Length);
			byte[] hash = new byte[20];
			GCHandle gch = GCHandle.Alloc(m_HashValue, GCHandleType.Pinned);
			IntPtr pointer = gch.AddrOfPinnedObject();
			Marshal.Copy(pointer, hash, 0, 20);
			gch.Free();
			return hash;
		}
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="MD2CryptoServiceProvider"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			try {
				GC.SuppressFinalize(this);
			} catch {}
			m_Disposed = true;
		}
		/// <summary>
		/// Finalizes the MD2CryptoServiceProvider.
		/// </summary>
		~RIPEMD160Managed() {
			Clear();
		}
		#region Compress Method
		private void Compress() {
			uint aa = m_HashValue[0],  bb = m_HashValue[1],  cc = m_HashValue[2],  dd = m_HashValue[3],  ee = m_HashValue[4];
			uint aaa = m_HashValue[0], bbb = m_HashValue[1], ccc = m_HashValue[2], ddd = m_HashValue[3], eee = m_HashValue[4];
			/* round 1 */
			FF(ref aa, bb, ref cc, dd, ee, m_X[ 0], 11);
			FF(ref ee, aa, ref bb, cc, dd, m_X[ 1], 14);
			FF(ref dd, ee, ref aa, bb, cc, m_X[ 2], 15);
			FF(ref cc, dd, ref ee, aa, bb, m_X[ 3], 12);
			FF(ref bb, cc, ref dd, ee, aa, m_X[ 4],  5);
			FF(ref aa, bb, ref cc, dd, ee, m_X[ 5],  8);
			FF(ref ee, aa, ref bb, cc, dd, m_X[ 6],  7);
			FF(ref dd, ee, ref aa, bb, cc, m_X[ 7],  9);
			FF(ref cc, dd, ref ee, aa, bb, m_X[ 8], 11);
			FF(ref bb, cc, ref dd, ee, aa, m_X[ 9], 13);
			FF(ref aa, bb, ref cc, dd, ee, m_X[10], 14);
			FF(ref ee, aa, ref bb, cc, dd, m_X[11], 15);
			FF(ref dd, ee, ref aa, bb, cc, m_X[12],  6);
			FF(ref cc, dd, ref ee, aa, bb, m_X[13],  7);
			FF(ref bb, cc, ref dd, ee, aa, m_X[14],  9);
			FF(ref aa, bb, ref cc, dd, ee, m_X[15],  8);
			/* round 2 */
			GG(ref ee, aa, ref bb, cc, dd, m_X[ 7],  7);
			GG(ref dd, ee, ref aa, bb, cc, m_X[ 4],  6);
			GG(ref cc, dd, ref ee, aa, bb, m_X[13],  8);
			GG(ref bb, cc, ref dd, ee, aa, m_X[ 1], 13);
			GG(ref aa, bb, ref cc, dd, ee, m_X[10], 11);
			GG(ref ee, aa, ref bb, cc, dd, m_X[ 6],  9);
			GG(ref dd, ee, ref aa, bb, cc, m_X[15],  7);
			GG(ref cc, dd, ref ee, aa, bb, m_X[ 3], 15);
			GG(ref bb, cc, ref dd, ee, aa, m_X[12],  7);
			GG(ref aa, bb, ref cc, dd, ee, m_X[ 0], 12);
			GG(ref ee, aa, ref bb, cc, dd, m_X[ 9], 15);
			GG(ref dd, ee, ref aa, bb, cc, m_X[ 5],  9);
			GG(ref cc, dd, ref ee, aa, bb, m_X[ 2], 11);
			GG(ref bb, cc, ref dd, ee, aa, m_X[14],  7);
			GG(ref aa, bb, ref cc, dd, ee, m_X[11], 13);
			GG(ref ee, aa, ref bb, cc, dd, m_X[ 8], 12);
			/* round 3 */
			HH(ref dd, ee, ref aa, bb, cc, m_X[ 3], 11);
			HH(ref cc, dd, ref ee, aa, bb, m_X[10], 13);
			HH(ref bb, cc, ref dd, ee, aa, m_X[14],  6);
			HH(ref aa, bb, ref cc, dd, ee, m_X[ 4],  7);
			HH(ref ee, aa, ref bb, cc, dd, m_X[ 9], 14);
			HH(ref dd, ee, ref aa, bb, cc, m_X[15],  9);
			HH(ref cc, dd, ref ee, aa, bb, m_X[ 8], 13);
			HH(ref bb, cc, ref dd, ee, aa, m_X[ 1], 15);
			HH(ref aa, bb, ref cc, dd, ee, m_X[ 2], 14);
			HH(ref ee, aa, ref bb, cc, dd, m_X[ 7],  8);
			HH(ref dd, ee, ref aa, bb, cc, m_X[ 0], 13);
			HH(ref cc, dd, ref ee, aa, bb, m_X[ 6],  6);
			HH(ref bb, cc, ref dd, ee, aa, m_X[13],  5);
			HH(ref aa, bb, ref cc, dd, ee, m_X[11], 12);
			HH(ref ee, aa, ref bb, cc, dd, m_X[ 5],  7);
			HH(ref dd, ee, ref aa, bb, cc, m_X[12],  5);
			/* round 4 */
			II(ref cc, dd, ref ee, aa, bb, m_X[ 1], 11);
			II(ref bb, cc, ref dd, ee, aa, m_X[ 9], 12);
			II(ref aa, bb, ref cc, dd, ee, m_X[11], 14);
			II(ref ee, aa, ref bb, cc, dd, m_X[10], 15);
			II(ref dd, ee, ref aa, bb, cc, m_X[ 0], 14);
			II(ref cc, dd, ref ee, aa, bb, m_X[ 8], 15);
			II(ref bb, cc, ref dd, ee, aa, m_X[12],  9);
			II(ref aa, bb, ref cc, dd, ee, m_X[ 4],  8);
			II(ref ee, aa, ref bb, cc, dd, m_X[13],  9);
			II(ref dd, ee, ref aa, bb, cc, m_X[ 3], 14);
			II(ref cc, dd, ref ee, aa, bb, m_X[ 7],  5);
			II(ref bb, cc, ref dd, ee, aa, m_X[15],  6);
			II(ref aa, bb, ref cc, dd, ee, m_X[14],  8);
			II(ref ee, aa, ref bb, cc, dd, m_X[ 5],  6);
			II(ref dd, ee, ref aa, bb, cc, m_X[ 6],  5);
			II(ref cc, dd, ref ee, aa, bb, m_X[ 2], 12);
			/* round 5 */
			JJ(ref bb, cc, ref dd, ee, aa, m_X[ 4],  9);
			JJ(ref aa, bb, ref cc, dd, ee, m_X[ 0], 15);
			JJ(ref ee, aa, ref bb, cc, dd, m_X[ 5],  5);
			JJ(ref dd, ee, ref aa, bb, cc, m_X[ 9], 11);
			JJ(ref cc, dd, ref ee, aa, bb, m_X[ 7],  6);
			JJ(ref bb, cc, ref dd, ee, aa, m_X[12],  8);
			JJ(ref aa, bb, ref cc, dd, ee, m_X[ 2], 13);
			JJ(ref ee, aa, ref bb, cc, dd, m_X[10], 12);
			JJ(ref dd, ee, ref aa, bb, cc, m_X[14],  5);
			JJ(ref cc, dd, ref ee, aa, bb, m_X[ 1], 12);
			JJ(ref bb, cc, ref dd, ee, aa, m_X[ 3], 13);
			JJ(ref aa, bb, ref cc, dd, ee, m_X[ 8], 14);
			JJ(ref ee, aa, ref bb, cc, dd, m_X[11], 11);
			JJ(ref dd, ee, ref aa, bb, cc, m_X[ 6],  8);
			JJ(ref cc, dd, ref ee, aa, bb, m_X[15],  5);
			JJ(ref bb, cc, ref dd, ee, aa, m_X[13],  6);
			/* parallel round 1 */
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 5],  8);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, m_X[14],  9);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 7],  9);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 0], 11);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 9], 13);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 2], 15);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, m_X[11], 15);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 4],  5);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, m_X[13],  7);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 6],  7);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, m_X[15],  8);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 8], 11);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 1], 14);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, m_X[10], 14);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 3], 12);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, m_X[12],  6);
			/* parallel round 2 */
			III(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 6],  9); 
			III(ref ddd, eee, ref aaa, bbb, ccc, m_X[11], 13);
			III(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 3], 15);
			III(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 7],  7);
			III(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 0], 12);
			III(ref eee, aaa, ref bbb, ccc, ddd, m_X[13],  8);
			III(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 5],  9);
			III(ref ccc, ddd, ref eee, aaa, bbb, m_X[10], 11);
			III(ref bbb, ccc, ref ddd, eee, aaa, m_X[14],  7);
			III(ref aaa, bbb, ref ccc, ddd, eee, m_X[15],  7);
			III(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 8], 12);
			III(ref ddd, eee, ref aaa, bbb, ccc, m_X[12],  7);
			III(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 4],  6);
			III(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 9], 15);
			III(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 1], 13);
			III(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 2], 11);
			/* parallel round 3 */
			HHH(ref ddd, eee, ref aaa, bbb, ccc, m_X[15],  9);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 5],  7);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 1], 15);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 3], 11);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 7],  8);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, m_X[14],  6);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 6],  6);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 9], 14);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, m_X[11], 12);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 8], 13);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, m_X[12],  5);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 2], 14);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, m_X[10], 13);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 0], 13);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 4],  7);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, m_X[13],  5);
			/* parallel round 4 */   
			GGG(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 8], 15);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 6],  5);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 4],  8);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 1], 11);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 3], 14);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, m_X[11], 14);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, m_X[15],  6);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 0], 14);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 5],  6);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, m_X[12],  9);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 2], 12);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, m_X[13],  9);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 9], 12);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 7],  5);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, m_X[10], 15);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, m_X[14],  8);
			/* parallel round 5 */
			FFF(ref bbb, ccc, ref ddd, eee, aaa, m_X[12],  8);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, m_X[15],  5);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, m_X[10], 12);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 4],  9);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 1], 12);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, m_X[ 5],  5);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, m_X[ 8], 14);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 7],  6);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 6],  8);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 2], 13);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, m_X[13],  6);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, m_X[14],  5);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, m_X[ 0], 15);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, m_X[ 3], 13);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, m_X[ 9], 11);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, m_X[11], 11);
			/* combine results */
			ddd += cc + m_HashValue[1];               /* final result for m_HashValue[0] */
			m_HashValue[1] = m_HashValue[2] + dd + eee;
			m_HashValue[2] = m_HashValue[3] + ee + aaa;
			m_HashValue[3] = m_HashValue[4] + aa + bbb;
			m_HashValue[4] = m_HashValue[0] + bb + ccc;
			m_HashValue[0] = ddd;
		}
		private void CompressFinal(ulong length) {
			uint lswlen = (uint)(length & 0xFFFFFFFF);
			uint mswlen = (uint)(length >> 32);
			// clear m_X
			Array.Clear(m_X, 0, m_X.Length);
			// put bytes from m_ExtraData into m_X
			int ptr = 0;
			for (uint i = 0; i < (lswlen & 63); i++) {
				// byte i goes into word X[i div 4] at pos.  8*(i mod 4)
				m_X[i >> 2] ^= ((uint)m_ExtraData[ptr++]) << (int)(8 * (i & 3));
			}
			// append the bit m_n == 1
			m_X[(lswlen >> 2) & 15] ^= (uint)1 << (int)(8 * (lswlen & 3) + 7);
			if ((lswlen & 63) > 55) {
				// length goes to next block
				Compress();
				Array.Clear(m_X, 0, m_X.Length);
			}
			// append length in bits
			m_X[14] = lswlen << 3;
			m_X[15] = (lswlen >> 29) | (mswlen << 3);
			Compress();
		}
		#endregion
		#region RIPEMD Core Functions
		private uint ROL(uint x, int n) {
			return (((x) << (n)) | ((x) >> (32-(n))));
		}
		private uint F(uint x, uint y, uint z) {
			return ((x) ^ (y) ^ (z)) ;
		}
		private uint G(uint x, uint y, uint z) {
			return (((x) & (y)) | (~(x) & (z)));
		}
		private uint H(uint x, uint y, uint z) {
			return (((x) | ~(y)) ^ (z));
		}
		private uint I(uint x, uint y, uint z) {
			return (((x) & (z)) | ((y) & ~(z)));
		}
		private uint J(uint x, uint y, uint z) {
			return ((x) ^ ((y) | ~(z)));
		}
		private void FF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += F(b, c, d) + x;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void GG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += G(b, c, d) + x + 0x5a827999;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void HH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += H(b, c, d) + x + 0x6ed9eba1;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void II(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += I(b, c, d) + x + 0x8f1bbcdc;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void JJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += J(b, c, d) + x + 0xa953fd4e;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void FFF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += F(b, c, d) + x;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void GGG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += G(b, c, d) + x + 0x7a6d76e9;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void HHH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += H(b, c, d) + x + 0x6d703ef3;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void III(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += I(b, c, d) + x + 0x5c4dd124;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void JJJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += J(b, c, d) + x + 0x50a28be6;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		#endregion
		/// <summary>
		/// A buffer that holds the extra data.
		/// </summary>
		private byte[] m_ExtraData;
		/// <summary>
		/// The X vectors.
		/// </summary>
		private uint[] m_X;
		/// <summary>
		/// The current value of the hash.
		/// </summary>
		private uint[] m_HashValue;
		/// <summary>
		/// The nubver of bytes hashed.
		/// </summary>
		private ulong m_Length;
		/// <summary>
		/// A boolean that indicates whether the object has been disposed or not.
		/// </summary>
		private bool m_Disposed;
	}
}
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
	/// Defines the basic operations of a unmanaged Rijndael cryptographic transformation.
	/// </summary>
	internal class RijndaelUnmanagedTransform : ICryptoTransform {
		/// <summary>
		/// Initializes a new instance of the RijndaelUnmanagedTransform class.
		/// </summary>
		/// <param name="algorithm">One of the <see cref="CryptoAlgorithm"/> values.</param>
		/// <param name="method">One of the <see cref="CryptoMethod"/> values.</param>
		/// <param name="key">The key to use.</param>
		/// <param name="iv">The IV to use.</param>
		/// <param name="mode">One of the <see cref="CipherMode"/> values.</param>
		/// <param name="feedback">The feedback size of the cryptographic operation in bits.</param>
		/// <param name="padding">One of the <see cref="PaddingMode"/> values.</param>
		public RijndaelUnmanagedTransform(CryptoAlgorithm algorithm, CryptoMethod method, byte[] key, byte[] iv, CipherMode mode, int feedback, PaddingMode padding) {
			m_Key = new SymmetricKey(CryptoProvider.RsaAes, algorithm, key);
			m_Key.IV = iv;
			m_Key.Mode = mode;
			if (mode == CipherMode.CFB)
				m_Key.FeedbackSize = feedback;
			m_Key.Padding = padding;
			m_BlockSize = 128;
			m_Method = method;
		}
		/// <summary>
		/// Releases all unmanaged resources.
		/// </summary>
		~RijndaelUnmanagedTransform() {
			Dispose();
		}
		/// <summary>
		/// Releases all unmanaged resources.
		/// </summary>
		public void Dispose() {
			if (m_Key != null) {
				m_Key.Dispose();
				m_Key = null;
			}
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		/// <value><b>true</b> if the current transform can be reused; otherwise, <b>false</b>.</value>
		public bool CanReuseTransform {
			get {
				if (m_Key == null)
					throw new ObjectDisposedException(this.GetType().FullName);
				return true;
			}
		}
		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		/// <value><b>true</b> if multiple blocks can be transformed; otherwise, <b>false</b>.</value>
		public bool CanTransformMultipleBlocks {
			get {
				if (m_Key == null)
					throw new ObjectDisposedException(this.GetType().FullName);
				return true;
			}
		}
		/// <summary>
		/// Gets the input block size.
		/// </summary>
		/// <value>The size of the input data blocks in bytes.</value>
		public int InputBlockSize {
			get {
				if (m_Key == null)
					throw new ObjectDisposedException(this.GetType().FullName);
				return m_BlockSize / 8;
			}
		}
		/// <summary>
		/// Gets the output block size.
		/// </summary>
		/// <value>The size of the output data blocks in bytes.</value>
		public int OutputBlockSize {
			get {
				if (m_Key == null)
					throw new ObjectDisposedException(this.GetType().FullName);
				return m_BlockSize / 8;
			}
		}
		/// <summary>
		/// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">The output to which to write the transform.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>The number of bytes written.</returns>
		/// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">One of the specified offsets or lengths is invalid.</exception>
		/// <exception cref="CryptographicException">An error occurs while transforming the specified data.</exception>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
			if (m_Key == null)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (inputBuffer == null || outputBuffer == null)
				throw new ArgumentNullException();
			if (inputCount < 0 || inputOffset < 0 || outputOffset < 0 ||inputOffset + inputCount > inputBuffer.Length)
				throw new ArgumentOutOfRangeException();
			int length = inputCount;
			if (m_Method == CryptoMethod.Encrypt) {
				if (SspiProvider.CryptEncrypt(m_Key.Handle, 0, 0, 0, null, ref length, 0) == 0)
					throw new CryptographicException("Could not encrypt data.");
				if (outputBuffer.Length - outputOffset < length)
					throw new ArgumentOutOfRangeException();
				Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
				length = inputCount;
				GCHandle h = GCHandle.Alloc(outputBuffer, GCHandleType.Pinned);
				try {
					if (SspiProvider.CryptEncrypt(m_Key.Handle, 0, 0, 0, new IntPtr(h.AddrOfPinnedObject().ToInt64() + outputOffset), ref length, outputBuffer.Length - outputOffset) == 0)
						throw new CryptographicException("Could not encrypt data.");
				} finally {
					h.Free();
				}
			} else { // decrypt
				byte[] orgCopy = new byte[inputCount];
				Array.Copy(inputBuffer, inputOffset, orgCopy, 0, inputCount);
				if (SspiProvider.CryptDecrypt(m_Key.Handle, 0, 0, 0, orgCopy, ref length) == 0)
					throw new CryptographicException("Could not decrypt data.");
				if (length > outputBuffer.Length - outputOffset)
					throw new ArgumentOutOfRangeException();
				Array.Copy(orgCopy, 0, outputBuffer, outputOffset, length);
			}
			return length;
		}
		/// <summary>
		/// Transforms the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input for which to compute the transform.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>The computed transform.</returns>
		/// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentOutOfRangeException">The combination of offset and length is invalid.</exception>
		/// <exception cref="CryptographicException">An error occurs while transforming the specified data.</exception>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount ) {
			if (m_Key == null)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (inputBuffer == null)
				throw new ArgumentNullException();
			if (inputCount < 0 || inputOffset < 0 ||inputOffset + inputCount > inputBuffer.Length)
				throw new ArgumentOutOfRangeException();
			int length = inputCount;
			byte[] ret;
			if (m_Method == CryptoMethod.Encrypt) {
				if (SspiProvider.CryptEncrypt(m_Key.Handle, 0, 1, 0, null, ref length, 0) == 0)
					throw new CryptographicException("Could not encrypt data.");
				ret = new byte[length];
				Array.Copy(inputBuffer, inputOffset, ret, 0, inputCount);
				length = inputCount;
				if (SspiProvider.CryptEncrypt(m_Key.Handle, 0, 1, 0, ret, ref length, ret.Length) == 0)
					throw new CryptographicException("Could not encrypt data.");
			} else { // decrypt
				byte[] orgCopy = new byte[inputCount];
				Array.Copy(inputBuffer, inputOffset, orgCopy, 0, inputCount);
				if (SspiProvider.CryptDecrypt(m_Key.Handle, 0, 1, 0, orgCopy, ref length) == 0)
					throw new CryptographicException("Could not decrypt data.");
				ret = new byte[length];
				Array.Copy(orgCopy, 0, ret, 0, length);
			}
			return ret;
		}
		/// <summary>
		/// Holds the block size of the algorithm.
		/// </summary>
		private int m_BlockSize;
		/// <summary>
		/// Holds the <see cref="SymmetricKey"/> used for the cryptographic transformations.
		/// </summary>
		private SymmetricKey m_Key;
		/// <summary>
		/// Holds the <see cref="CryptoMethod"/> for this cryptographic operation.
		/// </summary>
		private CryptoMethod m_Method;
	}
}
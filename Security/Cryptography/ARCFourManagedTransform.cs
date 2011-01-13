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
	/// Represents an ARCFour managed ICryptoTransform.
	/// </summary>
	internal sealed class ARCFourManagedTransform : ICryptoTransform {
		/// <summary>
		/// Initializes a new instance of the ARCFourManagedTransform class.
		/// </summary>
		/// <param name="key">The key used to initialize the ARCFour state.</param>
		public ARCFourManagedTransform(byte[] key) {
			m_Key = (byte[])key.Clone();
			m_KeyLen = key.Length;
			m_Permutation = new byte[256];
			m_Disposed = false;
			Init();
		}
		/// <summary>
		/// Gets a value indicating whether the current transform can be reused.
		/// </summary>
		/// <value>This property returns <b>true</b>.</value>
		public bool CanReuseTransform {
			get {
				return true;
			}
		}
		/// <summary>
		/// Gets a value indicating whether multiple blocks can be transformed.
		/// </summary>
		/// <value>This property returns <b>true</b>.</value>
		public bool CanTransformMultipleBlocks {
			get {
				return true;
			}
		}
		/// <summary>
		/// Gets the input block size.
		/// </summary>
		/// <value>This property returns 1.</value>
		public int InputBlockSize {
			get {
				return 1;
			}
		}
		/// <summary>
		/// Gets the output block size.
		/// </summary>
		/// <value>This property returns 1.</value>
		public int OutputBlockSize {
			get {
				return 1;
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
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/>, <paramref name="inputCount"/> or <paramref name="outputOffset"/> is invalid.</exception>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (inputBuffer == null || outputBuffer == null)
				throw new ArgumentNullException();
			if (inputOffset < 0 || outputOffset < 0 || inputOffset + inputCount > inputBuffer.Length || outputOffset + inputCount > outputBuffer.Length)
				throw new ArgumentOutOfRangeException();
			byte j, temp;
			int length = inputOffset + inputCount;
			for(; inputOffset < length; inputOffset++, outputOffset++) {
				// update indices
				m_Index1 = (byte)((m_Index1 + 1) % 256);
				m_Index2 = (byte)((m_Index2 + m_Permutation[m_Index1]) % 256);
				// swap m_State.permutation[m_State.index1] and m_State.permutation[m_State.index2]
				temp = m_Permutation[m_Index1];
				m_Permutation[m_Index1] = m_Permutation[m_Index2];
				m_Permutation[m_Index2] = temp;
				// transform byte
				j = (byte)((m_Permutation[m_Index1] + m_Permutation[m_Index2]) % 256);
				outputBuffer[outputOffset] = (byte)(inputBuffer[inputOffset] ^ m_Permutation[j]);
			}
			return inputCount;
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
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/> or <paramref name="inputCount"/> is invalid.</exception>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount ) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			byte[] ret = new byte[inputCount];
			TransformBlock(inputBuffer, inputOffset, inputCount, ret, 0);
			Init();
			return ret;
		}
		/// <summary>
		/// This method (re)initializes the cipher.
		/// </summary>
		private void Init() {
			byte temp;
			// init state variable
			for (int i = 0; i < 256; i++) {
				m_Permutation[i] = (byte)i; 
			}
			m_Index1 = 0;
			m_Index2 = 0;
			// randomize, using key
			for (int j = 0, i = 0; i < 256; i++) {
				j = (j + m_Permutation[i] + m_Key[i % m_KeyLen]) % 256;
				// swap m_State.permutation[i] and m_State.permutation[j]
				temp = m_Permutation[i];
				m_Permutation[i] = m_Permutation[j];
				m_Permutation[j] = temp;
			}
		}
		/// <summary>
		/// Disposes of the cryptographic parameters.
		/// </summary>
		public void Dispose() {
			Array.Clear(m_Key, 0, m_Key.Length);
			Array.Clear(m_Permutation, 0, m_Permutation.Length);
			m_Index1 = 0;
			m_Index2 = 0;
			m_Disposed = true;
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		/// <summary>
		/// Finalizes the object.
		/// </summary>
		~ARCFourManagedTransform() {
			Dispose();
		}
		/// <summary>
		/// Holds the key that is used to initialize the ARCFour state.
		/// </summary>
		private byte[] m_Key;
		/// <summary>
		/// Holds the length of the key, in bytes.
		/// </summary>
		private int m_KeyLen;
		/// <summary>
		/// Holds state information.
		/// </summary>
		private byte[] m_Permutation;
		/// <summary>
		/// Holds state information.
		/// </summary>
		private byte m_Index1;
		/// <summary>
		/// Holds state information.
		/// </summary>
		private byte m_Index2;
		/// <summary>
		/// Holds a boolean that indicates whether the class has been disposed of or not.
		/// </summary>
		private bool m_Disposed;
	}
}
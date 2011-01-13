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
	/// The DataProtectionCryptoServiceProvider is a class that performs encryption and decryption on data without explicitly requiring a password.
	/// <br>There are two different types of encryption. The first type will associate the encrypted data with the logon credentials of the current user. Hence only a user with matching logon credentials can decrypt the data.</br>
	/// <br>The second type will associate the encrypted data with the local machine. Any user of a specific machine will be able to decrypt encrypted data under this scheme.</br>
	/// <br>This class cannot be inherited.</br>
	/// </summary>
	public sealed class DataProtectionCryptoServiceProvider : IDisposable {
		/// <summary>
		/// Initializes a new <see cref="DataProtectionCryptoServiceProvider"/> instance.
		/// </summary>
		public DataProtectionCryptoServiceProvider() : this(null) {}
		/// <summary>
		/// Initializes a new <see cref="DataProtectionCryptoServiceProvider"/> instance.
		/// </summary>
		/// <param name="optionalEntropy">A buffer holding any additional entropy that can be used during encryption and decryption.</param>
		/// <remarks>The same entropy must be provided during the encryption and decryption process. Otherwise, the decryption will fail.</remarks>
		public DataProtectionCryptoServiceProvider(byte[] optionalEntropy) {
			if (optionalEntropy != null)
				m_OptionalEntropy = (byte[])optionalEntropy.Clone();
			m_Disposed = false;
		}
		/// <summary>
		/// Encrypts data according to a specified protection type.
		/// </summary>
		/// <param name="type">One of the <see cref="ProtectionType"/> values.</param>
		/// <param name="data">The data to encrypt.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>An array of encrypted bytes.</returns>
		/// <remarks>
		/// The number of the returned bytes will be larger than the number of input bytes.
		/// The method will use the entropy from the <see cref="Entropy"/> property.
		/// </remarks>
		public byte[] ProtectData(ProtectionType type, byte[] data) {
			return ProtectData(type, data, this.Entropy);
		}
		/// <summary>
		/// Encrypts data according to a specified protection type.
		/// </summary>
		/// <param name="type">One of the <see cref="ProtectionType"/> values.</param>
		/// <param name="data">The data to encrypt.</param>
		/// <param name="entropy">Additional entropy to use during the encyption process. This parameter can be set to null.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>An array of encrypted bytes.</returns>
		/// <remarks>The number of the returned bytes will be larger than the number of input bytes.</remarks>
		public byte[] ProtectData(ProtectionType type, byte[] data, byte[] entropy) {
			if (data == null)
				throw new ArgumentNullException();
			return ProtectData(type, data, 0, data.Length, entropy);
		}
		/// <summary>
		/// Encrypts data according to a specified protection type.
		/// </summary>
		/// <param name="type">One of the <see cref="ProtectionType"/> values.</param>
		/// <param name="data">The data to encrypt.</param>
		/// <param name="offset">The zero-based position in the <i>data</i> parameter at which to begin encrypting.</param>
		/// <param name="size">The number of bytes to encrypt.</param>
		/// <param name="entropy">Additional entropy to use during the encryption process. This parameter can be set to null.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified <i>offset</i> or <i>size</i> exceeds the size of buffer.</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>An array of encrypted bytes.</returns>
		/// <remarks>The number of the returned bytes will be larger than the number of input bytes.</remarks>
		public byte[] ProtectData(ProtectionType type, byte[] data, int offset, int size, byte[] entropy) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (data == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset + size > data.Length || !Enum.IsDefined(typeof(ProtectionType), type))
				throw new ArgumentException();
			DataBlob input = new DataBlob();
			DataBlob entr = new DataBlob();
			DataBlob output = new DataBlob();
			try {
				// initialize input structure
				input.cbData = size;
				input.pbData = Marshal.AllocHGlobal(size);
				Marshal.Copy(data, offset, input.pbData, size);
				// initialize entropy structure
				if (entropy == null) {
					entr.cbData = 0;
					entr.pbData = IntPtr.Zero;
				} else {
					entr.cbData = entropy.Length;
					entr.pbData = Marshal.AllocHGlobal(entr.cbData);
					Marshal.Copy(entropy, 0, entr.pbData, entr.cbData);
				}
				// initialize output structure
				output.cbData = 0;
				output.pbData = IntPtr.Zero;
				// call the function and check for errors
				int flags = 0;
				if (type == ProtectionType.LocalMachine)
					flags = flags | SecurityConstants.CRYPTPROTECT_LOCAL_MACHINE;
				if (!Environment.UserInteractive)
					flags = flags | SecurityConstants.CRYPTPROTECT_UI_FORBIDDEN;
				if (SspiProvider.CryptProtectData(ref input, "", ref entr, IntPtr.Zero, IntPtr.Zero, flags, ref output) == 0 || output.pbData == IntPtr.Zero)
					throw new CryptographicException("The data could not be protected.");
				byte[] ret = new byte[output.cbData];
				Marshal.Copy(output.pbData, ret, 0, output.cbData);
				return ret;
			} finally {
				if (input.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(input.pbData);
				if (entr.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(entr.pbData);
				if (output.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(output.pbData);
			}
		}
		/// <summary>
		/// Decrypts data that has been encrypted with the <see cref="ProtectData"/> method.
		/// </summary>
		/// <param name="data">The data to decrypt.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified <i>offset</i> or <i>size</i> exceeds the size of buffer.</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>The decrypted data.</returns>
		/// <remarks>
		/// The method will use the entropy from the <see cref="Entropy"/> property.
		/// The entropy used during decryption must be the same as the entropy used during encryption.
		/// </remarks>
		public byte[] UnprotectData(byte[] data) {
			return UnprotectData(data, this.Entropy);
		}
		/// <summary>
		/// Decrypts data that has been encrypted with the <see cref="ProtectData"/> method.
		/// </summary>
		/// <param name="data">The data to decrypt.</param>
		/// <param name="entropy">Additional entropy to use during the encyption process. This parameter can be set to null.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>The decrypted data.</returns>
		/// <remarks>The entropy used during decryption must be the same as the entropy used during encryption.</remarks>
		public byte[] UnprotectData(byte[] data, byte[] entropy) {
			if (data == null)
				throw new ArgumentNullException();
			return UnprotectData(data, 0, data.Length, entropy);
		}
		/// <summary>
		/// Decrypts data that has been encrypted with the <see cref="ProtectData"/> method.
		/// </summary>
		/// <param name="data">The data to decrypt.</param>
		/// <param name="offset">The zero-based position in the <i>data</i> parameter at which to begin decrypting.</param>
		/// <param name="size">The number of bytes to decrypt.</param>
		/// <param name="entropy">Additional entropy to use during the decryption process. This parameter can be set to null.</param>
		/// <exception cref="ArgumentNullException"><i>data</i> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified <i>offset</i> or <i>size</i> exceeds the size of buffer.</exception>
		/// <exception cref="CryptographicException">An error occurs during the encryption process. Under some circumstances, Microsoft cryptographic service providers may not allow encryption when used in France. This may occur on down-level platforms such as Windows 98 and Windows NT 4.0, depending on the system's configuration and the version of the CSPs.</exception>
		/// <returns>The decrypted data.</returns>
		/// <remarks>The entropy used during decryption must be the same as the entropy used during encryption.</remarks>
		public byte[] UnprotectData(byte[] data, int offset, int size, byte[] entropy) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (data == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset + size > data.Length)
				throw new ArgumentException();
			DataBlob input = new DataBlob();
			DataBlob entr = new DataBlob();
			DataBlob output = new DataBlob();
			try {
				// initialize input structure
				input.cbData = size;
				input.pbData = Marshal.AllocHGlobal(size);
				Marshal.Copy(data, offset, input.pbData, size);
				// initialize entropy structure
				if (entropy == null) {
					entr.cbData = 0;
					entr.pbData = IntPtr.Zero;
				} else {
					entr.cbData = entropy.Length;
					entr.pbData = Marshal.AllocHGlobal(entr.cbData);
					Marshal.Copy(entropy, 0, entr.pbData, entr.cbData);
				}
				// initialize output structure
				output.cbData = 0;
				output.pbData = IntPtr.Zero;
				// call the function and check for errors
				int flags = 0;
				if (!Environment.UserInteractive)
					flags = flags | SecurityConstants.CRYPTPROTECT_UI_FORBIDDEN;
				if (SspiProvider.CryptUnprotectData(ref input, IntPtr.Zero, ref entr, IntPtr.Zero, IntPtr.Zero, flags, ref output) == 0 || output.pbData == IntPtr.Zero)
					throw new CryptographicException("The data could not be unprotected.");
				byte[] ret = new byte[output.cbData];
				Marshal.Copy(output.pbData, ret, 0, output.cbData);
				return ret;
			} finally {
				if (input.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(input.pbData);
				if (entr.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(entr.pbData);
				if (output.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(output.pbData);
			}
		}
		/// <summary>
		/// Holds additional entropy that can be used during the encryption and decryption process.
		/// </summary>
		/// <value>An array en entropy bytes.</value>
		public byte[] Entropy {
			get {
				return m_OptionalEntropy;
			}
			set {
				m_OptionalEntropy = value;
			}
		}
		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="DataProtectionCryptoServiceProvider"/> class.
		/// </summary>
		public void Dispose() {
			if (m_OptionalEntropy != null)
				Array.Clear(m_OptionalEntropy, 0, m_OptionalEntropy.Length);
			m_Disposed = true;
		}
		/// <summary>
		/// Finalizes the <see cref="DataProtectionCryptoServiceProvider"/> class.
		/// </summary>
		~DataProtectionCryptoServiceProvider() {
			Dispose();
		}
		/// <summary>
		/// Holds the entropy.
		/// </summary>
		private byte[] m_OptionalEntropy;
		/// <summary>
		/// Holds a value that indicates whether the class has been disposed of or not.
		/// </summary>
		private bool m_Disposed;
	}
}
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
	/// Accesses the unmanaged version of the <see cref="Rijndael"/> algorithm. This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// This class will use the unmanaged implementation of the Rijndael algorithm, when possible. If the unmanaged Rijndael algorithm is not available, it will fall back to the <see cref="RijndaelManaged"/> implementation.
	/// </remarks>
	public sealed class RijndaelCryptoServiceProvider : Rijndael {
		/// <summary>
		/// Initializes a new instance of the <see cref="RijndaelCryptoServiceProvider"/> class.
		/// </summary>
		public RijndaelCryptoServiceProvider() {
			// acquire an AES context
			try {
				m_Provider = CAPIProvider.Handle;
				if (CAPIProvider.HandleProviderType != SecurityConstants.PROV_RSA_AES)
					m_Provider = 0;
			} catch {
				m_Provider = 0;
			}
/*			if (SspiProvider.CryptAcquireContext(ref m_Provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_AES, 0) == 0) {
				if (Marshal.GetLastWin32Error() == SecurityConstants.NTE_BAD_KEYSET)
					SspiProvider.CryptAcquireContext(ref m_Provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_AES, SecurityConstants.CRYPT_NEWKEYSET);
			}*/
			m_Managed = new RijndaelManaged();
		}
		/// <summary>
		/// Releases all unmanaged resources.
		/// </summary>
		~RijndaelCryptoServiceProvider() {
			Dispose(true);
		}
		/// <summary>
		/// Releases all unmanaged resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (m_Managed != null) {
				m_Managed.Clear();
				m_Managed = null;
			}
/*			if (m_Provider != 0) {
				SspiProvider.CryptReleaseContext(m_Provider, 0);
				m_Provider = 0;
			}*/
			try {
				GC.SuppressFinalize(this);
			} catch {}
			m_Disposed = true;
		}
		/// <summary>
		/// Gets or sets the block size of the cryptographic operation in bits.
		/// </summary>
		/// <value>The block size in bits.</value>
		/// <exception cref="CryptographicException">The block size is invalid.</exception>
		/// <remarks>The block size is the basic unit of data that can be encrypted or decrypted in one operation. Messages longer than the block size are handled as successive blocks; messages shorter than the block size must be padded with extra bits to reach the size of a block. Valid block sizes are determined by the symmetric algorithm used.</remarks>
		public override int BlockSize {
			get {
				return m_Managed.BlockSize;
			}
			set {
				m_Managed.BlockSize = value;
			}
		}
		/// <summary>
		/// Gets or sets the feedback size of the cryptographic operation in bits.
		/// </summary>
		/// <value>The feedback size in bits.</value>
		/// <exception cref="CryptographicException">The feedback size is larger than the block size.</exception>
		/// <remarks>The feedback size determines the amount of data that is fed back to successive encryption or decryption operations. The feedback size cannot be greater than the block size.</remarks>
		public override int FeedbackSize {
			get {
				return m_Managed.FeedbackSize;
			}
			set {
				m_Managed.FeedbackSize = value;
			}
		}
		/// <summary>
		/// Gets or sets the initialization vector (IV) for the symmetric algorithm.
		/// </summary>
		/// <value>The initialization vector.</value>
		/// <exception cref="ArgumentNullException">An attempt is made to set the IV to a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CryptographicException">An attempt is made to set the IV to an invalid size.</exception>
		/// <remarks>If this property is a null reference (<b>Nothing</b> in Visual Basic) when it is used, <see cref="GenerateIV"/> is called to create a new random value.</remarks>
		public override byte[] IV {
			get {
				return m_Managed.IV;
			}
			set {
				m_Managed.IV = value;
			}
		}
		/// <summary>
		/// Gets or sets the secret key for the symmetric algorithm.
		/// </summary>
		/// <value>The secret key to be used for the symmetric algorithm.</value>
		/// <exception cref="ArgumentNullException">An attempt is made to set the key to a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <remarks>
		/// <p>The secret key is used both for encryption and for decryption. For a symmetric algorithm to be secure, the secret key must be known only to the sender and the receiver. The valid key sizes are specified by the particular symmetric algorithm implementation and are listed in <see cref="LegalKeySizes"/>.</p>
		/// <p>If this property is a null reference (<b>Nothing</b> in Visual Basic) when it is used, <see cref="GenerateKey"/> is called to create a new random value.</p>
		/// </remarks>
		public override byte[] Key {
			get {
				return m_Managed.Key;
			} 
			set {
				m_Managed.Key = value;
			}
		}
		/// <summary>
		/// Gets or sets the size of the secret key used by the symmetric algorithm in bits.
		/// </summary>
		/// <value>The size of the secret key used by the symmetric algorithm.</value>
		/// <exception cref="CryptographicException">The key size is not valid.</exception>
		/// <remarks>The valid key sizes are specified by the particular symmetric algorithm implementation and are listed in <see cref="LegalKeySizes"/>.</remarks>
		public override int KeySize {
			get {
				return m_Managed.KeySize;
			} 
			set {
				m_Managed.KeySize = value;
			}
		}
		/// <summary>
		/// Gets the block sizes that are supported by the symmetric algorithm.
		/// </summary>
		/// <value>An array containing the block sizes supported by the algorithm.</value>
		/// <remarks>Only block sizes that match an entry in this array are supported by the symmetric algorithm.</remarks>
		public override KeySizes[] LegalBlockSizes {
			get {
				return m_Managed.LegalBlockSizes;
			}
		}
		/// <summary>
		/// Gets the key sizes that are supported by the symmetric algorithm.
		/// </summary>
		/// <value>An array containing the key sizes supported by the algorithm.</value>
		/// <remarks>Only key sizes that match an entry in this array are supported by the symmetric algorithm.</remarks>
		public override KeySizes[] LegalKeySizes {
			get {
				return m_Managed.LegalKeySizes;
			}
		}
		/// <summary>
		/// Gets or sets the mode for operation of the symmetric algorithm.
		/// </summary>
		/// <value>The mode for operation of the symmetric algorithm.</value>
		/// <exception cref="CryptographicException">The cipher mode is not one of the CipherMode values.</exception>
		/// <remarks>See CipherMode for a description of specific modes.</remarks>
		public override CipherMode Mode {
			get {
				return m_Managed.Mode;
			}
			set {
				m_Managed.Mode = value;
			}
		}
		/// <summary>
		/// Gets or sets the padding mode used in the symmetric algorithm.
		/// </summary>
		/// <value>The padding mode used in the symmetric algorithm.</value>
		/// <exception cref="CryptographicException">The padding mode is not one of the PaddingMode values.</exception>
		/// <remarks>Most plain text messages do not consist of a number of bytes that completely fill blocks. Often, there are not enough bytes to fill the last block. When this happens, a padding string is added to the text. For example, if the block length is 64 bits and the last block contains only 40 bits, 24 bits of padding are added. See <see cref="PaddingMode"/> for a description of specific modes.</remarks>
		public override PaddingMode Padding {
			get {
				return m_Managed.Padding;
			}
			set {
				m_Managed.Padding = value;
			}
		}
		/// <summary>
		/// Generates a random initialization vector (IV) to be used for the algorithm.
		/// </summary>
		/// <remarks>Use this method to generate a random IV when none is specified.</remarks>
		public override void GenerateIV() {
			m_Managed.GenerateIV();
		}
		/// <summary>
		/// Generates a random Key to be used for the algorithm.
		/// </summary>
		/// <remarks>Use this method to generate a random key when none is specified.</remarks>
		public override void GenerateKey() {
			m_Managed.GenerateKey();
		}
		/// <summary>
		/// Creates a symmetric <see cref="Rijndael"/> decryptor object with the specified <see cref="Key"/> and initialization vector (<see cref="IV"/>).
		/// </summary>
		/// <param name="rgbKey">The secret key to be used for the symmetric algorithm.</param>
		/// <param name="rgbIV">The IV to be used for the symmetric algorithm.</param>
		/// <returns>A symmetric Rijndael decryptor object.</returns>
		/// <remarks>This method decrypts an encrypted message created using the <see cref="CreateEncryptor"/> overload with the same signature.</remarks>
		public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (rgbKey == null || rgbIV == null)
				throw new ArgumentNullException();
			if (this.Mode == CipherMode.CTS || this.Mode == CipherMode.OFB || this.Mode == CipherMode.CFB)
				throw new CryptographicException(this.Mode.ToString() + " is not supported by this implementation.");
			try {
				if (CanUseUnmanaged(rgbKey.Length * 8, rgbIV.Length * 8, this.Padding))
					return new RijndaelUnmanagedTransform(GetKeyType(rgbKey.Length * 8), CryptoMethod.Decrypt, rgbKey, rgbIV, this.Mode, this.FeedbackSize, this.Padding);
			} catch {}
			return m_Managed.CreateDecryptor(rgbKey, rgbIV);
		}
		/// <summary>
		/// Creates a symmetric <see cref="Rijndael"/> encryptor object with the specified <see cref="Key"/> and initialization vector (<see cref="IV"/>).
		/// </summary>
		/// <param name="rgbKey">The secret key to be used for the symmetric algorithm.</param>
		/// <param name="rgbIV">The IV to be used for the symmetric algorithm.</param>
		/// <returns>A symmetric Rijndael encryptor object.</returns>
		/// <remarks>Use the <see cref="CreateDecryptor"/> overload with the same signature to decrypt the result of this method.</remarks>
		public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) {
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().FullName);
			if (rgbKey == null || rgbIV == null)
				throw new ArgumentNullException();
			if (this.Mode == CipherMode.CTS || this.Mode == CipherMode.OFB || this.Mode == CipherMode.CFB)
				throw new CryptographicException(this.Mode.ToString() + " is not supported by this implementation.");
			try {
				if (CanUseUnmanaged(rgbKey.Length * 8, rgbIV.Length * 8, this.Padding))
					return new RijndaelUnmanagedTransform(GetKeyType(rgbKey.Length * 8), CryptoMethod.Encrypt, rgbKey, rgbIV, this.Mode, this.FeedbackSize, this.Padding);
			} catch {}
			return m_Managed.CreateEncryptor(rgbKey, rgbIV);
		}
		/// <summary>
		/// Converts a key size into an instance of the <see cref="CryptoAlgorithm"/> enum.
		/// </summary>
		/// <param name="size">The size of the key, in bits.</param>
		/// <returns>One of the CryptoAlgorithm values.</returns>
		/// <exception cref="CryptographicException"><paramref name="size"/> is invalid.</exception>
		private CryptoAlgorithm GetKeyType(int size) {
			if (size == 128)
				return CryptoAlgorithm.Rijndael128;
			else if (size == 192)
				return CryptoAlgorithm.Rijndael192;
			else if (size == 256)
				return CryptoAlgorithm.Rijndael256;
			else 
				throw new CryptographicException("Invalid keysize!");
		}
		/// <summary>
		/// Gets the <see cref="CryptoAlgorithm"/> value that corresponds with the value of the <see cref="KeySize"/> property.
		/// </summary>
		/// <returns>One of the CryptoAlgorithm values.</returns>
		private CryptoAlgorithm KeyType {
			get {
				return GetKeyType(this.KeySize);
			}
		}
		/// <summary>
		/// Returns a value that indicates whether the unmanaged Rijndael implementation can be used with the specified parameters.
		/// </summary>
		/// <param name="keySize">A value that indicates the size of the key, in bits.</param>
		/// <param name="blockSize">A value that indicates the size of the blocks, in bits.</param>
		/// <param name="padding">One of the <see cref="PaddingMode"/> values.</param>
		/// <returns><b>true</b> if the unmanaged implementation can be used, <b>false</b> otherwise.</returns>
		private bool CanUseUnmanaged(int keySize, int blockSize, PaddingMode padding) {
			return (m_Provider != 0) &&		// make sure the unmanaged AES CSP is available
				(blockSize == 128) &&		// although Rijndael supports block sizes of 128, 192 and 256 bits,
				// the AES only specifies a blocksize of 128 bits [and only this block size is supported by the unmanaged implementation]
				(padding == PaddingMode.PKCS7 || padding == PaddingMode.None) && // only PKCS7 padding is supported
				(keySize == 128 || keySize == 192 || keySize == 256); // make sure we use one of the supported key lengths
		}
		/// <summary>
		/// Returns a value that indicates whether the unmanaged Rijndael implementation can be used with the current parameters.
		/// </summary>
		/// <returns><b>true</b> if the unmanaged implementation can be used, <b>false</b> otherwise.</returns>
		private bool CanUseUnmanaged() {
			return CanUseUnmanaged(this.KeySize, this.BlockSize, this.Padding);
		}
		/// <summary>Holds a managed <see cref="Rijndael"/> instance.</summary>
		private RijndaelManaged m_Managed;
		/// <summary>Handle of the unmanaged AES CSP.</summary>
		private int m_Provider;
		/// <summary>Holds a boolean that indicates whether this object has been disposed.</summary>
		private bool m_Disposed;
	}
}
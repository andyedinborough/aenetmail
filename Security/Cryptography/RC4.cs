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
	/// Represents the base class from which all implementations of the RC4 symmetric stream cipher must inherit.
	/// </summary>
	/// <remarks>
	/// RC4 is a trademark of RSA Data Security Inc.
	/// </remarks>
	public abstract class RC4 : SymmetricAlgorithm {
		/// <summary>
		/// Initializes a new instance of the RC4 class.
		/// </summary>
		/// <remarks>
		/// The default keysize is 128 bits.
		/// </remarks>
		public RC4() {
			this.KeySizeValue = 128;
		}
		/// <summary>
		/// Gets or sets the block size of the cryptographic operation in bits.
		/// </summary>
		/// <value>The block size of RC4 is always 8 bits.</value>
		/// <exception cref="CryptographicException">The block size is invalid.</exception>
		public override int BlockSize {
			get {
				return 8;
			}
			set {
				if (value != 8 && value != 0)
					throw new CryptographicException("RC4 is a stream cipher, not a block cipher.");
			}
		}
		/// <summary>
		/// Gets or sets the feedback size of the cryptographic operation in bits.
		/// </summary>
		/// <value>This property always throws a <see cref="CryptographicException"/>.</value>
		/// <exception cref="CryptographicException">This exception is always thrown.</exception>
		/// <remarks>RC4 doesn't use the FeedbackSize property.</remarks>
		public override int FeedbackSize {
			get {
				throw new CryptographicException("RC4 doesn't use the feedback size property.");
			}
			set {
				throw new CryptographicException("RC4 doesn't use the feedback size property.");
			}
		}
		/// <summary>
		/// Gets or sets the initialization vector (IV) for the symmetric algorithm.
		/// </summary>
		/// <value>This property always returns a byte array of length one. The value of the byte in the array is always set to zero.</value>
		/// <exception cref="CryptographicException">An attempt is made to set the IV to an invalid instance.</exception>
		/// <remarks>RC4 doesn't use the IV property, however the property accepts IV's of up to one byte (RC4's <see cref="BlockSize"/>) in order to interoperate with software that has been written with the use of block ciphers in mind.</remarks>
		public override byte[] IV {
			get {
				return new byte[1];
			}
			set {
				if (value != null && value.Length > 1)
					throw new CryptographicException("RC4 doesn't use an Initialization Vector.");
			}
		}
		/// <summary>
		/// Gets the block sizes that are supported by the symmetric algorithm.
		/// </summary>
		/// <value>An array containing the block sizes supported by the algorithm.</value>
		/// <remarks>Only a block size of one byte is supported by the RC4 algorithm.</remarks>
		public override KeySizes[] LegalBlockSizes {
			get {
				return new KeySizes[] { new KeySizes(8, 8, 0) };
			}
		}
		/// <summary>
		/// Gets the key sizes that are supported by the symmetric algorithm.
		/// </summary>
		/// <value>An array containing the key sizes supported by the algorithm.</value>
		/// <remarks>Only key sizes that match an entry in this array are supported by the symmetric algorithm.</remarks>
		public override KeySizes[] LegalKeySizes {
			get {
				return new KeySizes[] { new KeySizes(8, 2048, 8) };
			}
		}
		/// <summary>
		/// Gets or sets the mode for operation of the symmetric algorithm.
		/// </summary>
		/// <value>The mode for operation of the symmetric algorithm.</value>
		/// <remarks>RC4 only supports the OFB cipher mode. See <see cref="CipherMode"/> for a description of this mode.</remarks>
		/// <exception cref="CryptographicException">The cipher mode is not OFB.</exception>
		public override CipherMode Mode {
			get {
				return CipherMode.OFB;
			}
			set {
				if (value != CipherMode.OFB)
					throw new CryptographicException("RC4 only supports OFB.");
			}
		}
		/// <summary>
		/// Gets or sets the padding mode used in the symmetric algorithm.
		/// </summary>
		/// <value>The padding mode used in the symmetric algorithm. This property always returns PaddingMode.None.</value>
		/// <exception cref="CryptographicException">The padding mode is set to a padding mode other than PaddingMode.None.</exception>
		public override PaddingMode Padding {
			get {
				return PaddingMode.None;
			}
			set {
				if (value != PaddingMode.None)
					throw new CryptographicException("RC4 is a stream cipher, not a block cipher.");
			}
		}
		/// <summary>
		/// This is a stub method.
		/// </summary>
		/// <remarks>Since the RC4 cipher doesn't use an Initialization Vector, this method will not do anything.</remarks>
		public override void GenerateIV() {
			// do nothing
		}
		/// <summary>
		/// Generates a random Key to be used for the algorithm.
		/// </summary>
		/// <remarks>Use this method to generate a random key when none is specified.</remarks>
		public override void GenerateKey() {
			byte[] key = new byte[this.KeySize / 8];
			GetRNGCSP().GetBytes(key);
			this.Key = key;
		}
		/// <summary>
		/// Creates an instance of the default cryptographic object used to perform the RC4 transformation.
		/// </summary>
		/// <returns>The instance of a cryptographic object used to perform the RC4 transformation.</returns>
		public static new RC4 Create() {
			return Create("ARCFOUR");
		}
		/// <summary>
		/// Creates an instance of the specified cryptographic object used to perform the RC4 transformation.
		/// </summary>
		/// <param name="AlgName">The name of the specific implementation of <see cref="RC4"/> to create.</param>
		/// <returns>A cryptographic object.</returns>
		public static new RC4 Create(string AlgName) {
			try {
				if (AlgName.ToUpper() == "RC4" || AlgName.ToLower() == "org.mentalis.security.cryptography.rc4cryptoserviceprovider")
					return new RC4CryptoServiceProvider();
				else if (AlgName.ToUpper() == "ARCFOUR" || AlgName.ToLower() == "org.mentalis.security.cryptography.arcfourmanaged")
					return new ARCFourManaged();
			} catch {}
			return null;
		}
		/// <summary>
		/// Returns an <see cref="RNGCryptoServiceProvider"/> instance.
		/// </summary>
		/// <returns>An RNGCryptoServiceProvider instance.</returns>
		protected RNGCryptoServiceProvider GetRNGCSP() {
			if (m_RNG == null)
				m_RNG = new RNGCryptoServiceProvider();
			return m_RNG;
		}
		/// <summary>
		/// Holds the RNGCryptoServiceProvider object.
		/// </summary>
		private RNGCryptoServiceProvider m_RNG;
	}
}
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
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Org.Mentalis.Security.Cryptography {
	internal class CAPIProvider {
		internal CAPIProvider() {}
		public static int Handle {
			get {
				m_Provider.CreateInternalHandle(ref m_Provider.m_Handle, null);
				return m_Provider.m_Handle;
			}
		}
		public static int HandleProviderType {
			get {
				m_Provider.CreateInternalHandle(ref m_Provider.m_Handle, null);
				return m_Provider.m_HandleProviderType;
			}
		}
		public static int ContainerHandle {
			get {
				m_Provider.CreateInternalHandle(ref m_Provider.m_ContainerHandle, SecurityConstants.KEY_CONTAINER);
				return m_Provider.m_ContainerHandle;
			}
		}
		public void CreateInternalHandle(ref int handle, string container) {
			if (handle == 0) {
				lock(this) {
					if (handle == 0 && !m_Error) {
						int flags, fs = 0, fmk = 0;
						if (!Environment.UserInteractive && Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5) {
							fs = SecurityConstants.CRYPT_SILENT;
							fmk = SecurityConstants.CRYPT_MACHINE_KEYSET;
						}
						for(int i = 0; i < m_Providers.Length; i++) {
							flags = fs | fmk;
							m_HandleProviderType = m_Providers[i];
							if (SspiProvider.CryptAcquireContext(ref handle, container, null, m_Providers[i], flags) == 0) {
								if (Marshal.GetLastWin32Error() == SecurityConstants.NTE_BAD_KEYSET) {
									SspiProvider.CryptAcquireContext(ref handle, container, null, m_Providers[i], flags | SecurityConstants.CRYPT_NEWKEYSET);
								} else if(fmk != 0) {
									flags = fs;
									if (SspiProvider.CryptAcquireContext(ref handle, container, null, m_Providers[i], flags) == 0) {
										if (Marshal.GetLastWin32Error() == SecurityConstants.NTE_BAD_KEYSET) {
											SspiProvider.CryptAcquireContext(ref handle, container, null, m_Providers[i], flags | SecurityConstants.CRYPT_NEWKEYSET);
										}
									}
								}
							}
							if (handle != 0)
								break;
						}
						if (handle == 0) {
							m_Error = true;
							m_HandleProviderType = 0;
						}
					}
					if (m_Error)
						throw new CryptographicException("Couldn't acquire crypto service provider context.");
				}
			}
		}
		~CAPIProvider() {
			if (m_Handle != 0)
				SspiProvider.CryptReleaseContext(m_Handle, 0);
			if (m_ContainerHandle != 0)
				SspiProvider.CryptReleaseContext(m_ContainerHandle, 0);
		}
		private int m_Handle = 0;
		private int m_ContainerHandle = 0;
		private bool m_Error = false;
		private int m_HandleProviderType = 0;
		private static int[] m_Providers = new int[] {SecurityConstants.PROV_RSA_AES, SecurityConstants.PROV_RSA_FULL};
		private static CAPIProvider m_Provider = new CAPIProvider();
	}
	/// <summary>
	/// Specifies the type of encryption method to use when protecting data.
	/// </summary>
	public enum ProtectionType {
		/// <summary>The encrypted data is associated with the local machine. Any user on the computer on which the data is encrypted can decrypt the data.</summary>
		LocalMachine,
		/// <summary>The encrypted data is associated with the current user. Only a user with logon credentials matching those of the encrypter can decrypt the data.</summary>
		CurrentUser
	}
	/// <summary>
	/// Specifies the type of algorithm to be used when performing unmanaged cryptographic transformations.
	/// </summary>
	internal enum CryptoAlgorithm : int {
		/// <summary>The Rijndael algorithm with a key size of 128 bits.</summary>
		Rijndael128 = SecurityConstants.CALG_AES_128,
		/// <summary>The Rijndael algorithm with a key size of 192 bits.</summary>
		Rijndael192 = SecurityConstants.CALG_AES_192,
		/// <summary>The Rijndael algorithm with a key size of 256 bits.</summary>
		Rijndael256 = SecurityConstants.CALG_AES_256,
		/// <summary>The RC4 algorithm.</summary>
		RC4 = SecurityConstants.CALG_RC4
	}
	/// <summary>
	/// Specifies the type of CSP to be used when performing unmanaged cryptographic transformations.
	/// </summary>
	internal enum CryptoProvider {
		/// <summary>Microsoft's full RSA CSP.</summary>
		RsaFull = SecurityConstants.PROV_RSA_FULL,
		/// <summary>Microsoft's full RSA CSP that supports the AES.</summary>
		RsaAes = SecurityConstants.PROV_RSA_AES
	}
	/// <summary>
	/// Specifies the type of transformation for a cryptographic operation.
	/// </summary>
	internal enum CryptoMethod {
		/// <summary>Encrypt the data.</summary>
		Encrypt,
		/// <summary>Decrypt the data.</summary>
		Decrypt
	}
	/// <summary>
	/// The PUBLICKEYSTRUC structure, also known as the BLOBHEADER structure, indicates a key's BLOB type and the algorithm that the key uses. One of these structures is located at the beginning of the pbData member of every key BLOB.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct PUBLICKEYSTRUC {
		/// <summary>Key BLOB type. The only BLOB types currently defined are PUBLICKEYBLOB, PRIVATEKEYBLOB, SIMPLEBLOB, and PLAINTEXTBLOB. Other key BLOB types will be defined as needed. </summary>
		public byte bType;
		/// <summary>Version number of the key BLOB format. This currently must always have a value of CUR_BLOB_VERSION (0x02).</summary>
		public byte bVersion;
		/// <summary>WORD reserved for future use. Must be set to zero.</summary>
		public short reserved;
		/// <summary>Algorithm identifier for the key contained by the key BLOB. Some examples are CALG_RSA_SIGN, CALG_RSA_KEYX, CALG_RC2, and CALG_RC4.</summary>
		public IntPtr aiKeyAlg;
	}
}
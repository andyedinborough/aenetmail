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
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Org.Mentalis.Security;

namespace Org.Mentalis.Security.Ssl.Shared {
	/// <summary>
	/// We use this class because there's a bug in the RSACryptoServiceProvider class that disallows encrypting
	/// more than 16 bytes on Windows 98, ME and NT4.
	/// </summary>
	internal class RSAKeyTransform {
		public RSAKeyTransform(RSACryptoServiceProvider key) {
			FillStaticInfo();
			m_Key = key;
			m_Disposed = false;
		}
		public byte[] CreateKeyExchange(byte[] data) {
			if (m_Disposed)
				throw new CryptographicException("The key has been disposed");
			if (m_NeedsHack) {
				// get the key handle
				IntPtr key = GetHandle(m_Key);
				// encrypt the data
				int size = data.Length;
				SspiProvider.CryptEncrypt(key, 0, 1, 0, null, ref size, 0);
				byte[] ret = new byte[size];
				Array.Copy(data, 0, ret, 0, data.Length);
				size = data.Length;
				if (SspiProvider.CryptEncrypt(key, 0, 1, 0, ret, ref size, ret.Length) == 0)
					throw new CryptographicException("Unable to decrypt the key exchange.");
				Array.Reverse(ret); // little endian to big endian
				return ret;
			} else {
				return m_Key.Encrypt(data, false);
			}
		}
		public byte[] DecryptKeyExchange(byte[] data) {
			if (m_Disposed)
				throw new CryptographicException("The key has been disposed");
			if (m_NeedsHack) {
				// get the key handle
				IntPtr key = GetHandle(m_Key);
				// decrypt the data
				byte[] dec = (byte[])data.Clone();
				Array.Reverse(dec); // big endian to little endian
				int size = data.Length;
				if (SspiProvider.CryptDecrypt(key, 0, 1, 0, dec, ref size) == 0)
					throw new CryptographicException("Unable to decrypt the key exchange [" + Marshal.GetLastWin32Error() + "].");
				byte[] ret = new byte[size];
				Array.Copy(dec, 0, ret, 0, size);
				return ret;
			} else {
				return m_Key.Decrypt(data, false);
			}
		}
		private bool m_Disposed;
		private RSACryptoServiceProvider m_Key;
		
		// static stuff
		private static IntPtr GetHandle(RSACryptoServiceProvider rsa) {
			if (m_KeyProp != null) {
				return (IntPtr)m_KeyProp.GetValue(rsa, null);
			} else {
				return (IntPtr)m_KeyField.GetValue(rsa);
			}
		}
		private static void FillStaticInfo() {
			if (!m_FillInfoDone) {
				m_FillInfoDone = true;
				if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5)
					m_NeedsHack = false;
				else
					m_NeedsHack = true;
				if (m_NeedsHack) {
					Type t = typeof(RSACryptoServiceProvider);
					m_KeyProp = t.GetProperty("hKey", BindingFlags.Instance | BindingFlags.NonPublic); // CLR 1.0
					if (m_KeyProp == null) {
						m_KeyField = t.GetField("_hKey", BindingFlags.Instance | BindingFlags.NonPublic); // CLR 1.1
						if(m_KeyField == null) {
							m_NeedsHack = false; // newer version of CLR..?
						}
					}
				}
			}
		}
		private static bool m_FillInfoDone = false;
		private static PropertyInfo m_KeyProp = null;
		private static FieldInfo m_KeyField = null;
		private static bool m_NeedsHack;
	}
}
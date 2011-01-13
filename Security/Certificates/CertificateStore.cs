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
using System.IO;
using System.Text;
using System.Security;
using System.Collections;
using System.Runtime.InteropServices;

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Defines a certificate store.
	/// </summary>
	public class CertificateStore {
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs whil reading from the specified file.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		public static CertificateStore CreateFromPfxFile(string file, string password) {
			return CreateFromPfxFile(GetFileContents(file), password);
		}
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs whil reading from the specified file.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		public static CertificateStore CreateFromPfxFile(string file, string password, bool exportable) {
			return CreateFromPfxFile(GetFileContents(file), password, exportable);
		}
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <param name="location">One of the <see cref="KeysetLocation"/> values.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs whil reading from the specified file.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		public static CertificateStore CreateFromPfxFile(string file, string password, bool exportable, KeysetLocation location) {
			return CreateFromPfxFile(GetFileContents(file), password, exportable, location);
		}
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		public static CertificateStore CreateFromPfxFile(byte[] file, string password) {
			return CreateFromPfxFile(file, password, false);
		}
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		public static CertificateStore CreateFromPfxFile(byte[] file, string password, bool exportable) {
			return CreateFromPfxFile(file, password, exportable, KeysetLocation.Default);
		}
		/// <summary>
		/// Creates a new certificate store from a PFX/P12 encoded file.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <param name="location">One of the <see cref="KeysetLocation"/> values.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <exception cref="CertificateException">An error occurs while loading the PFX file.</exception>
		// Thanks go out to Chris Hudel for the implementation of this method.
		public static CertificateStore CreateFromPfxFile(byte[] file, string password, bool exportable, KeysetLocation location) {
			if (password == null || file == null)
				throw new ArgumentNullException("The arguments cannot be null references.");
			CertificateStore cs;
			DataBlob pPFX = new DataBlob();
			// Crypt_data_blob contains two elements,
			// cbData = the size of the blob
			// pbData = a byte array of [cbData] size containing contents of the .p12 file
			pPFX.cbData = file.Length;
			// We need to marshal the byte array Bytes into a pointer so that it can be placed
			// in the structure (class) for the WinAPI call
			IntPtr buffer = Marshal.AllocHGlobal(file.Length);
			Marshal.Copy(file, 0, buffer, file.Length);
			pPFX.pbData = buffer;
			// IF this really is a valid PFX file, then do some work on it 
			try {
				if (SspiProvider.PFXIsPFXBlob(ref pPFX) != 0) {
					if (SspiProvider.PFXVerifyPassword(ref pPFX, password, 0) != 0) {
						int flags = (int)location;
						if (exportable)
							flags |= SecurityConstants.CRYPT_EXPORTABLE;
						IntPtr m_Handle = SspiProvider.PFXImportCertStore(ref pPFX, password, flags);
						if (m_Handle.Equals(IntPtr.Zero)) {
							throw new CertificateException("Unable to import the PFX file! [error code = " + Marshal.GetLastWin32Error() + "]");
						}
						cs = new CertificateStore(m_Handle);
					} else {
						throw new ArgumentException("The specified password is invalid.");
					}
				} else {
					throw new CertificateException("The specified file is not a PFX file.");
				}
			} finally {
				// Free the pointer
				Marshal.FreeHGlobal(buffer);
			}
			return cs;
		}
		/// <summary>
		/// Returns the contents of a file.
		/// </summary>
		/// <param name="file">The file to read from.</param>
		/// <returns>A byte array with the contents of the specified file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while reading from the specified file.</exception>
		internal static byte[] GetFileContents(string file) {
			if (file == null)
				throw new ArgumentNullException();
			byte[] ret;
			try {
				FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				ret = new byte[fs.Length];
				int read = fs.Read(ret, 0, ret.Length);
				while(read < fs.Length) {
					read += fs.Read(ret, read, ret.Length - read);
				}
				fs.Close();
			} catch (Exception e) {
				throw new IOException("An error occurs while reading from the file.", e);
			}
			return ret;
		}
		/// <summary>
		/// Creates a new certificate store from a certificate file.
		/// </summary>
		/// <param name="file">The certificate file.</param>
		/// <returns>A <see cref="CertificateStore"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading the certificate.</exception>
		/// <remarks>The provider opens the file and first attempts to read the file as a serialized store, then as a PKCS #7 signed message, and finally as a single encoded certificate.</remarks>
		public static CertificateStore CreateFromCerFile(string file) {
			if (file == null)
				throw new ArgumentNullException("The filename cannot be a null reference.");
			IntPtr cs = SspiProvider.CertOpenStore(new IntPtr(SecurityConstants.CERT_STORE_PROV_FILENAME_A), SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, 0, file);
			if (cs == IntPtr.Zero)
				throw new CertificateException("An error occurs while opening the specified store.");
			return new CertificateStore(cs);
		}
		/// <summary>
		/// Duplicates an exisiting <see cref="CertificateStore"/>.
		/// </summary>
		/// <param name="store">The store to duplicate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public CertificateStore(CertificateStore store) {
			if (store == null)
				throw new ArgumentNullException();
			InitStore(store.m_Handle, true);
		}
		/// <summary>
		/// Initializes a new <see cref="CertificateStore"/> from a given handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the CertificateStore from.</param>
		/// <remarks>The handle will not be duplicated; when this CertificateStore instance is garbage collected, the handle will be freed.</remarks>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		public CertificateStore(IntPtr handle) : this(handle, false) {}
		/// <summary>
		/// Initializes a new <see cref="CertificateStore"/> from a given handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the CertificateStore from.</param>
		/// <param name="duplicate"><b>true</b> if the handle should be duplicated, <b>false</b> otherwise.</param>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		public CertificateStore(IntPtr handle, bool duplicate) {
			InitStore(handle, duplicate);
		}
		/// <summary>
		/// Initializes a new <see cref="CertificateStore"/> from a given store name.
		/// </summary>
		/// <param name="store">The name of the system store to open.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while opening the specified store.</exception>
		/// <remarks>
		/// <p>If the system store name provided in this parameter is not the name of an existing system store, a new system store will be created and used.</p>
		/// <p>Some example system stores are listed in the following table. 
		///	<list type="table">
		///		<listheader>
		///			<term>Predefined system store name</term>
		///			<description>Meaning</description>
		///		</listheader>
		///		<item>
		///			<term>"CA"</term>
		///			<description>Certification authority certificates.</description>
		///		</item>
		///		<item>
		///			<term>"MY"</term>
		///			<description>A certificate store holding "My" certificates with their associated private keys.</description>
		///		</item>
		///		<item>
		///			<term>"ROOT"</term>
		///			<description>Root certificates.</description>
		///		</item>
		///		<item>
		///			<term>"SPC"</term>
		///			<description>Software publisher certificates.</description>
		///		</item>
		///	</list></p>
		/// </remarks>
		public CertificateStore(string store) : this(StoreLocation.Users, store) {}
		/// <summary>
		/// Initializes a new <see cref="CertificateStore"/> from a given store name and a given store location.
		/// </summary>
		/// <param name="location">The location of the store.</param>
		/// <param name="store">The name of the store to open.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while opening the specified store.</exception>
		public CertificateStore(StoreLocation location, string store) {
			if (store == null)
				throw new ArgumentNullException("The name of the store cannot be a null reference.");
			m_Handle = SspiProvider.CertOpenStore(new IntPtr(SecurityConstants.CERT_STORE_PROV_SYSTEM_A), 0, 0, (int)location, store);
			if (m_Handle == IntPtr.Zero)
				throw new CertificateException("An error occurs while opening the specified store.");
		}
		/// <summary>
		/// Initializes a new temporary <see cref="CertificateStore"/> in memory.
		/// </summary>
		/// <remarks>If the store is closed, all the data in the store is lost.</remarks>
		/// <exception cref="CertificateException">An error occurs while creating the store.</exception>
		public CertificateStore() {
			m_Handle = SspiProvider.CertOpenStore(new IntPtr(SecurityConstants.CERT_STORE_PROV_MEMORY), SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, 0, null);
			if (m_Handle == IntPtr.Zero)
				throw new CertificateException("An error occurs while creating the store.");
		}
		/// <summary>
		/// Initializes a new temporary <see cref="CertificateStore"/> in memory and adds the specified certificates to it.
		/// </summary>
		/// <param name="certs">A set of certificates.</param>
		/// <remarks>If the store is closed, all the data in the store is lost.</remarks>
		/// <exception cref="InvalidCastException"><i>certs</i> contains at least one object that is not of type <see cref="Certificate"/>.</exception>
		public CertificateStore(IEnumerable certs) : this() {
			IEnumerator enm = certs.GetEnumerator();
			while(enm.MoveNext()) {
				AddCertificate((Certificate)enm.Current);
			}
		}
		/// <summary>
		/// Opens a serialized certificate store or a certificate store with signed PKCS7 messages.
		/// </summary>
		/// <param name="buffer">The bytes of the store to open.</param>
		/// <param name="type">One of the <see cref="CertificateStoreType"/> values.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while opening the store.</exception>
		public CertificateStore(byte[] buffer, CertificateStoreType type) {
			if (buffer == null)
				throw new ArgumentNullException();
			DataBlob data = new DataBlob();
			data.cbData = buffer.Length;
			data.pbData = Marshal.AllocHGlobal(data.cbData);
			Marshal.Copy(buffer, 0, data.pbData, buffer.Length);
			IntPtr provider;
			if (type == CertificateStoreType.Pkcs7Message)
				provider = new IntPtr(SecurityConstants.CERT_STORE_PROV_PKCS7);
			else
				provider = new IntPtr(SecurityConstants.CERT_STORE_PROV_SERIALIZED);
			m_Handle = SspiProvider.CertOpenStoreData(provider, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, IntPtr.Zero, 0, ref data);
			Marshal.FreeHGlobal(data.pbData);
			if (m_Handle == IntPtr.Zero)
				throw new CertificateException("An error occurs while opening the store.");
		}
		/// <summary>
		/// Initializes a new <see cref="CertificateStore"/> from a given handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the CertificateStore from.</param>
		/// <param name="duplicate"><b>true</b> if the handle should be duplicated, <b>false</b> otherwise.</param>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		protected void InitStore(IntPtr handle, bool duplicate) {
			if (handle == IntPtr.Zero)
				throw new ArgumentException("Invalid certificate store handle!");
			if(duplicate)
				m_Handle = SspiProvider.CertDuplicateStore(handle);
			else
				m_Handle = handle;
		}
		/// <summary>
		/// Returns the first certificate from the <see cref="CertificateStore"/>.
		/// </summary>
		/// <returns>A <see cref="Certificate"/> from the store.</returns>
		public Certificate FindCertificate() {
			return FindCertificate((Certificate)null);
		}
		/// <summary>
		/// Returns a certificate from the <see cref="CertificateStore"/>.
		/// </summary>
		/// <param name="previous">The previous certificate.</param>
		/// <returns>The <see cref="Certificate"/> that comes after <paramref name="previous"/> -or- a null reference (<b>Nothing in Visual Basic</b>) if there is no certificate after <paremref name="previous"/>.</returns>
		public Certificate FindCertificate(Certificate previous) {
			IntPtr prev;
			if (previous == null)
				prev = IntPtr.Zero;
			else
				prev = SspiProvider.CertDuplicateCertificateContext(previous.Handle);
			IntPtr ret = SspiProvider.CertFindCertificateInStore(Handle, SecurityConstants.X509_ASN_ENCODING, 0, SecurityConstants.CERT_FIND_ANY, IntPtr.Zero, prev);
			if (ret.Equals(IntPtr.Zero))
				return null;
			else
				return new Certificate(ret, this);
		}
		/// <summary>
		/// Finds a certificate having an enhanced key extension that matches one of the <paramref name="keyUsage"/> members.
		/// </summary>
		/// <param name="keyUsage">The list of enhanced key usages to search for.</param>
		/// <returns>A <see cref="Certificate"/> that has at least one of the specified key usages -or- a null reference (<b>Nothing in Visual Basic</b>) if no valid certificate could be found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyUsage"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="keyUsage"/> is invalid.</exception>
		public Certificate FindCertificateByUsage(string[] keyUsage) {
			return FindCertificateByUsage(keyUsage, null);
		}
		/// <summary>
		/// Finds a certificate having an enhanced key extension that matches one of the <paramref name="keyUsage"/> members.
		/// </summary>
		/// <param name="keyUsage">The list of enhanced key usages to search for.</param>
		/// <param name="previous">The previous certificate.</param>
		/// <returns>The <see cref="Certificate"/> that comes after <paramref name="previous"/> and that has at least one of the specified key usages -or- a null reference (<b>Nothing in Visual Basic</b>) if no other valid certificate could be found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyUsage"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="keyUsage"/> is invalid.</exception>
		public Certificate FindCertificateByUsage(string[] keyUsage, Certificate previous) {
			//   "1.3.6.1.5.5.7.3.1" is the Server Authentication OID as defined in RFC2459
			if (keyUsage == null)
				throw new ArgumentNullException();
			if (keyUsage.Length == 0)
				throw new ArgumentException();
			int total = 0;
			for(int i = 0; i < keyUsage.Length; i++) {
				if (keyUsage[i] == null || keyUsage[i].Length == 0)
					throw new ArgumentException();
				total += keyUsage[i].Length + 1;
			}
			IntPtr storage = Marshal.AllocHGlobal(total); // block of memory that contains all the strings
			IntPtr list = Marshal.AllocHGlobal(IntPtr.Size * keyUsage.Length); // list of pointers to the strings
			total = 0;
			IntPtr s = storage;
			for(int i = 0; i < keyUsage.Length; i++) {
				Marshal.Copy(Encoding.ASCII.GetBytes(keyUsage[i] + "\0"), 0, s, keyUsage[i].Length + 1);
				Marshal.WriteIntPtr(list, i * IntPtr.Size, s);
				s = new IntPtr(storage.ToInt64() + keyUsage[i].Length + 1);
			}
			// search for a certificate
			TrustListUsage usage = new TrustListUsage();
			usage.cUsageIdentifier = keyUsage.Length;
			usage.rgpszUsageIdentifier = list;
			IntPtr prev;
			if (previous == null)
				prev = IntPtr.Zero;
			else
				prev = SspiProvider.CertDuplicateCertificateContext(previous.Handle);
			IntPtr ret = SspiProvider.CertFindUsageCertificateInStore(Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, SecurityConstants.CERT_FIND_CTL_USAGE, ref usage, prev);
			Marshal.FreeHGlobal(list);
			Marshal.FreeHGlobal(storage);
			if (ret.Equals(IntPtr.Zero))
				return null;
			else
				return new Certificate(ret, this);
		}
		/// <summary>
		/// Finds a certificate with a matching hash.
		/// </summary>
		/// <param name="hash">The hash to search for.</param>
		/// <returns>The <see cref="Certificate"/> with the matching default hash -or- a null reference (<b>Nothing</b> in Visual Basic) if no certificate with that hash could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="hash"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public Certificate FindCertificateByHash(byte[] hash) {
			return FindCertificateByHash(hash, HashType.Default);
		}	
		/// <summary>
		/// Finds a certificate with a matching hash.
		/// </summary>
		/// <param name="hash">The hash to search for.</param>
		/// <param name="hashType">One of the HashType values.</param>
		/// <returns>The <see cref="Certificate"/> with the matching hash -or- a null reference (<b>Nothing</b> in Visual Basic) if no certificate with that hash could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="hash"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public Certificate FindCertificateByHash(byte[] hash, HashType hashType) {
			if (hash == null)
				throw new ArgumentNullException();
			int findType;
			if (hashType == HashType.SHA1)
				findType = SecurityConstants.CERT_FIND_SHA1_HASH;
			else if (hashType == HashType.MD5)
				findType = SecurityConstants.CERT_FIND_MD5_HASH;
			else
				findType = SecurityConstants.CERT_FIND_HASH;
			DataBlob data = new DataBlob();
			data.cbData = hash.Length;
			data.pbData = Marshal.AllocHGlobal(hash.Length);
			Marshal.Copy(hash, 0, data.pbData, hash.Length);
			IntPtr cert = SspiProvider.CertFindDataBlobCertificateInStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, findType, ref data, IntPtr.Zero);
			Marshal.FreeHGlobal(data.pbData);
			if (cert == IntPtr.Zero)
				return null;
			else
				return new Certificate(cert);
		}
		/// <summary>
		/// Finds a certificate with a matching key identifier.
		/// </summary>
		/// <param name="keyID">The key identifier to search for.</param>
		/// <returns>The <see cref="Certificate"/> with the matching key identifier -or- a null reference (<b>Nothing</b> in Visual Basic) if no matching certificate could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyID"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="keyID"/> is invalid.</exception>
		public Certificate FindCertificateByKeyIdentifier(byte[] keyID) {
			if (keyID == null)
				throw new ArgumentNullException();
			if (keyID.Length == 0)
				throw new ArgumentException();
			DataBlob data = new DataBlob();
			data.cbData = keyID.Length;
			data.pbData = Marshal.AllocHGlobal(keyID.Length);
			Marshal.Copy(keyID, 0, data.pbData, keyID.Length);
			IntPtr cert = SspiProvider.CertFindDataBlobCertificateInStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, SecurityConstants.CERT_FIND_KEY_IDENTIFIER, ref data, IntPtr.Zero);
			Marshal.FreeHGlobal(data.pbData);
			if (cert == IntPtr.Zero)
				return null;
			else
				return new Certificate(cert);
		}
		/// <summary>
		/// Finds a certificate with a matching subject name.
		/// </summary>
		/// <param name="name">The X500 string to search for.</param>
		/// <returns>A <see cref="Certificate"/> with a matching subject name -or- a null reference (<b>Nothing</b> in Visual Basic) if no matching certificate could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is invalid.</exception>
		public Certificate FindCertificateBySubjectName(string name) {
			return FindCertificateBySubjectName(name, null);
		}
		/// <summary>
		/// Finds a certificate with a matching subject name.
		/// </summary>
		/// <param name="name">The X500 string to search for.</param>
		/// <param name="previous">The previous certificate.</param>
		/// <returns>A <see cref="Certificate"/> with a matching subject name -or- a null reference (<b>Nothing</b> in Visual Basic) if no matching certificate could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is invalid.</exception>
		/// <exception cref="CertificateException">An error occurs while encoding the specified string.</exception>
		public Certificate FindCertificateBySubjectName(string name, Certificate previous) {
			if (name == null)
				throw new ArgumentNullException();
			if (name.Length == 0)
				throw new ArgumentException();
			IntPtr prev, cert = IntPtr.Zero;
			if (previous == null)
				prev = IntPtr.Zero;
			else
				prev = SspiProvider.CertDuplicateCertificateContext(previous.Handle);
			DataBlob data = new DataBlob();
			if (SspiProvider.CertStrToName(SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, name, SecurityConstants.CERT_X500_NAME_STR, IntPtr.Zero, IntPtr.Zero, ref data.cbData, IntPtr.Zero) == 0)
				throw new CertificateException("Could not encode the specified string. [is the string a valid X500 string?]");
			data.pbData = Marshal.AllocHGlobal(data.cbData);
			try {
				if (SspiProvider.CertStrToName(SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, name, SecurityConstants.CERT_X500_NAME_STR, IntPtr.Zero, data.pbData, ref data.cbData, IntPtr.Zero) == 0)
					throw new CertificateException("Could not encode the specified string.");
				cert = SspiProvider.CertFindDataBlobCertificateInStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, SecurityConstants.CERT_FIND_SUBJECT_NAME, ref data, prev);
			} finally {
				Marshal.FreeHGlobal(data.pbData);
			}
			if (cert == IntPtr.Zero)
				return null;
			else
				return new Certificate(cert);
		}
		/// <summary>
		/// Finds a certificate with a subject that contains a specified string.
		/// </summary>
		/// <param name="subject">The string to search for.</param>
		/// <returns>A <see cref="Certificate"/> with a matching subject string -or- a null reference (<b>Nothing</b> in Visual Basic) if no matching certificate could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="subject"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="subject"/> is invalid.</exception>
		/// <remarks>The string matching algorithm used is case-insensitive.</remarks>
		public Certificate FindCertificateBySubjectString(string subject) {
			return FindCertificateBySubjectString(subject, null);
		}
		/// <summary>
		/// Finds a certificate with a subject that contains a specified string.
		/// </summary>
		/// <param name="subject">The string to search for.</param>
		/// <param name="previous">The previous certificate.</param>
		/// <returns>A <see cref="Certificate"/> with a matching subject string -or- a null reference (<b>Nothing</b> in Visual Basic) if no matching certificate could be found in the store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="subject"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="subject"/> is invalid.</exception>
		/// <remarks>The string matching algorithm used is case-insensitive.</remarks>
		public Certificate FindCertificateBySubjectString(string subject, Certificate previous) {
			if (subject == null)
				throw new ArgumentNullException();
			if (subject.Length == 0)
				throw new ArgumentException();
			IntPtr prev;
			if (previous == null)
				prev = IntPtr.Zero;
			else
				prev = SspiProvider.CertDuplicateCertificateContext(previous.Handle);
			IntPtr cert = SspiProvider.CertFindStringCertificateInStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, 0, SecurityConstants.CERT_FIND_SUBJECT_STR_W, subject, prev);
			if (cert == IntPtr.Zero)
				return null;
			else
				return new Certificate(cert);
		}
		/// <summary>
		/// Enumerates all the certificates in the store.
		/// </summary>
		/// <returns>An array of <see cref="Certificate"/> instances.</returns>
		public Certificate[] EnumCertificates() {
			ArrayList certs = new ArrayList();
			Certificate prev = FindCertificate();
			while(prev != null) {
				certs.Add(prev);
				prev = FindCertificate(prev);
			}
			return (Certificate[])certs.ToArray(typeof(Certificate));
		}
		/// <summary>
		/// Enumerates all the certificates in the store.
		/// </summary>
		/// <param name="keyUsage">The list of enhanced key usages to search for.</param>
		/// <returns>An array of <see cref="Certificate"/> instances.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyUsage"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="keyUsage"/> is invalid.</exception>
		public Certificate[] EnumCertificates(string[] keyUsage) {
			ArrayList certs = new ArrayList();
			Certificate prev = FindCertificateByUsage(keyUsage);
			while(prev != null) {
				certs.Add(prev);
				prev = FindCertificateByUsage(keyUsage, prev);
			}
			return (Certificate[])certs.ToArray(typeof(Certificate));
		}
		/// <summary>
		/// Saves the <see cref="CertificateStore"/> as a PFX encoded file.
		/// </summary>
		/// <param name="filename">The filename of the new PFX file.</param>
		/// <param name="password">The password to use when encrypting the private keys.</param>
		/// <param name="exportPrivateKeys"><b>true</b> if the private keys should be exported [if possible], <b>false</b> otherwise.</param>
		/// <remarks>If the specified file already exists, the method will throw an exception.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while exporting the certificate store.</exception>
		/// <exception cref="IOException">An error occurs while writing the data to the file.</exception>
		public void ToPfxFile(string filename, string password, bool exportPrivateKeys) {
			SaveToFile(GetPfxBuffer(password, exportPrivateKeys), filename);
		}
		/// <summary>
		/// Saves the <see cref="CertificateStore"/> as a PFX encoded file.
		/// </summary>
		/// <param name="password">The password to use when encrypting the private keys.</param>
		/// <param name="exportPrivateKeys"><b>true</b> if the private keys should be exported [if possible], <b>false</b> otherwise.</param>
		/// <returns>An array of bytes that represents the PFX encoded store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while exporting the certificate store.</exception>
		public byte[] ToPfxBuffer(string password, bool exportPrivateKeys) {
			return GetPfxBuffer(password, exportPrivateKeys);
		}
		/// <summary>
		/// Returns the byte representation of the PFX encoded store.
		/// </summary>
		/// <param name="password">The password to use when encrypting the private keys.</param>
		/// <param name="exportPrivateKeys"><b>true</b> if the private keys should be exported [if possible], <b>false</b> otherwise.</param>
		/// <returns>An array of bytes that represents the PFX encoded store.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		private byte[] GetPfxBuffer(string password, bool exportPrivateKeys) {
			if (password == null)
				throw new ArgumentNullException();
			DataBlob data = new DataBlob();
			try {
				data.pbData = IntPtr.Zero;
				data.cbData = 0;
				if (SspiProvider.PFXExportCertStoreEx(Handle, ref data, password, IntPtr.Zero, exportPrivateKeys ? SecurityConstants.EXPORT_PRIVATE_KEYS : 0) == 0)
					throw new CertificateException("Could not export the certificate store.");
				data.pbData = Marshal.AllocHGlobal(data.cbData);
				if (SspiProvider.PFXExportCertStoreEx(Handle, ref data, password, IntPtr.Zero, exportPrivateKeys ? SecurityConstants.EXPORT_PRIVATE_KEYS : 0) == 0)
					throw new CertificateException("Could not export the certificate store.");
				byte[] buffer = new byte[data.cbData];
				Marshal.Copy(data.pbData, buffer, 0, buffer.Length);
				return buffer;
			} finally {
				if (data.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(data.pbData);
			}
		}
		/// <summary>
		/// Saves the <see cref="CertificateStore"/> in a file.
		/// </summary>
		/// <param name="filename">The filename of the serialized store.</param>
		/// <param name="type">One of the <see cref="CertificateStoreType"/> values.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while saving the store to the memory buffer.</exception>
		/// <exception cref="IOException">An error occurs while writing the data.</exception>
		public void ToCerFile(string filename, CertificateStoreType type) {
			SaveToFile(GetCerBuffer(type), filename);
		}
		/// <summary>
		/// Saves the <see cref="CertificateStore"/> in a buffer.
		/// </summary>
		/// <param name="type">One of the <see cref="CertificateStoreType"/> values.</param>
		/// <returns>An array of bytes that represents the encoded store.</returns>
		/// <exception cref="CertificateException">An error occurs while saving the store to the memory buffer.</exception>
		public byte[] ToCerBuffer(CertificateStoreType type) {
			return GetCerBuffer(type);
		}
		/// <summary>
		/// Returns the byte representation of the serialized store.
		/// </summary>
		/// <param name="type">One of the <see cref="CertificateStoreType"/> values.</param>
		/// <returns>An array of bytes that represents the encoded store.</returns>
		/// <exception cref="CertificateException">An error occurs while saving the store to the memory buffer.</exception>
		private byte[] GetCerBuffer(CertificateStoreType type) {
			DataBlob data = new DataBlob();
			try {
				data.cbData = 0;
				data.pbData = IntPtr.Zero;
				if (SspiProvider.CertSaveStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, (int)type, SecurityConstants.CERT_STORE_SAVE_TO_MEMORY, ref data, 0) == 0)
					throw new CertificateException("Unable to get the certificate data.");
				data.pbData = Marshal.AllocHGlobal(data.cbData);
				if (SspiProvider.CertSaveStore(this.Handle, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, (int)type, SecurityConstants.CERT_STORE_SAVE_TO_MEMORY, ref data, 0) == 0)
					throw new CertificateException("Unable to get the certificate data.");
				byte[] ret = new byte[data.cbData];
				Marshal.Copy(data.pbData, ret, 0, data.cbData);
				return ret;
			} finally {
				if (data.pbData != IntPtr.Zero)
					Marshal.FreeHGlobal(data.pbData);
			}
		}
		/// <summary>
		/// Writes a buffer with data to a file.
		/// </summary>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="filename">The filename to write the data to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> or <paramref name="filename"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while writing the data.</exception>
		private void SaveToFile(byte[] buffer, string filename) {
			if (filename == null || buffer == null)
				throw new ArgumentNullException();
			try {
				FileStream fs = File.Open(filename, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				fs.Write(buffer, 0, buffer.Length);
				fs.Close();
			} catch (Exception e) {
				throw new IOException("Could not write data to file.", e);
			}
		}
		/// <summary>
		/// Adds a <see cref="Certificate"/> to the <see cref="CertificateStore"/>.
		/// </summary>
		/// <param name="cert">The certificate to add to the store.</param>
		/// <exception cref="ArgumentNullException"><paramref name="cert"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while adding the certificate to the store.</exception>
		public void AddCertificate(Certificate cert) {
			if (cert == null)
				throw new ArgumentNullException();
			if (SspiProvider.CertAddCertificateContextToStore(this.Handle, cert.Handle, SecurityConstants.CERT_STORE_ADD_NEW, IntPtr.Zero) == 0) {
				if (Marshal.GetLastWin32Error() != SecurityConstants.CRYPT_E_EXISTS)
					throw new CertificateException("An error occurs while adding the certificate to the store.");
			}
		}
		/// <summary>
		/// Deletes a <see cref="Certificate"/> from the <see cref="CertificateStore"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="cert"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while removing the certificate from the store.</exception>
		public void DeleteCertificate(Certificate cert) {
			if (cert == null)
				throw new ArgumentNullException();
			Certificate sci = FindCertificateByHash(cert.GetCertHash(HashType.SHA1), HashType.SHA1);
			if (sci == null)
				throw new CertificateException("The certificate could not be found in the store.");
			if (SspiProvider.CertDeleteCertificateFromStore(SspiProvider.CertDuplicateCertificateContext(sci.Handle)) == 0)
				throw new CertificateException("An error occurs while removing the certificate from the store.");
		}
		/// <summary>
		/// Gets the handle of the CertificateStore.
		/// </summary>
		/// <value>An IntPtr that represents the handle of the certificate.</value>
		/// <remarks>The handle returned by this property should not be closed. If the handle is closed by an external actor, the methods of the CertificateStore object may fail in undocumented ways [for instance, an Access Violation may occur].</remarks>
		public IntPtr Handle {
			get {
				return m_Handle;
			}
		}
		/// <summary>
		/// Disposes of the <see cref="CertificateStore"/>.
		/// </summary>
		~CertificateStore() {
			Dispose();
		}
		/// <summary>
		/// Disposes of the <see cref="CertificateStore"/>.
		/// </summary>
		internal void Dispose() {
			if (m_Handle != IntPtr.Zero) {
				SspiProvider.CertCloseStore(m_Handle, 0);
				m_Handle = IntPtr.Zero;
			}
			try {
				GC.SuppressFinalize(this);
			} catch {}
		}
		/// <summary>
		/// Returns a CertificateStore from a list of cached stores. If the store is not yet cached, it will be created first.
		/// </summary>
		/// <param name="name">The name of the store.</param>
		/// <returns>The cached store.</returns>
		internal static CertificateStore GetCachedStore(string name) {
			CertificateStore cs = null;
			lock(m_CachedStores) {
				cs = m_CachedStores[name] as CertificateStore;
				if (cs == null) {
					cs = new CertificateStore(name);
					m_CachedStores.Add(name, cs);
				}
			}
			return cs;
		}
		/// <summary>
		/// Holds the handle of the certificate store.
		/// </summary>
		private IntPtr m_Handle;
		/// <summary>Represents the predefined system certificate store "CA". This field is constant.</summary>
		public const string CAStore = "CA";
		/// <summary>Represents the predefined system certificate store "My". This field is constant.</summary>
		public const string MyStore = "My";
		/// <summary>Represents the predefined system certificate store "Root". This field is constant.</summary>
		public const string RootStore = "Root";
		/// <summary>Represents the predefined system certificate store "Trust". This field is constant.</summary>
		public const string TrustStore = "Trust";
		/// <summary>Represents the untrusted certificate store. This field is constant.</summary>
		public const string UnTrustedStore = "Disallowed";
		/// <summary>Represents the software publisher certificate store. This field is constant.</summary>
		public const string SoftwarePublisherStore = "SPC";
		/// <summary>
		/// Holds the cached stores
		/// </summary>
		private static Hashtable m_CachedStores = new Hashtable();
	}
}

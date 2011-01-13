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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Defines a X509 v3 encoded certificate.
	/// </summary>
	public class Certificate : ICloneable {
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(string file, string password) {
			return CertificateStore.CreateFromPfxFile(file, password).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(string file, string password, bool exportable) {
			return CertificateStore.CreateFromPfxFile(file, password, exportable).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The full path to the PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <param name="location">One of the <see cref="KeysetLocation"/> values.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified file.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(string file, string password, bool exportable, KeysetLocation location) {
			return CertificateStore.CreateFromPfxFile(file, password, exportable, location).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified bytes.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(byte[] file, string password) {
			return CertificateStore.CreateFromPfxFile(file, password).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified bytes.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(byte[] file, string password, bool exportable) {
			return CertificateStore.CreateFromPfxFile(file, password, exportable).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a PFX file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The contents of a PFX file.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private keys associated with the certificates should be marked as exportable, <b>false</b> otherwise.</param>
		/// <param name="location">One of the <see cref="KeysetLocation"/> values.</param>
		/// <returns>One of the certificates in the PFX file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified bytes.</exception>
		/// <exception cref="ArgumentException"><paramref name="password"/> is invalid.</exception>
		/// <remarks>
		/// Warning: this method returns the first Certificate it can find in the specified PFX file.
		/// Care should be taken to verify whether the correct Certificate instance is returned
		/// when using PFX files that contain more than one certificate.
		/// For more fine-grained control over which certificate is returned, use
		/// the CertificateStore.CreateFromPfxFile method to instantiate a CertificateStore object
		/// and then use the CertificateStore.FindCertificateBy*** methods.
		/// </remarks>
		public static Certificate CreateFromPfxFile(byte[] file, string password, bool exportable, KeysetLocation location) {
			return CertificateStore.CreateFromPfxFile(file, password, exportable, location).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by opening a certificate file and retrieving the first certificate from it.
		/// </summary>
		/// <param name="file">The full path to the certificate file to open.</param>
		/// <returns>One of the certificates in the certificate file.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading certificates from the specified file.</exception>
		public static Certificate CreateFromCerFile(string file) {
			return CertificateStore.CreateFromCerFile(file).FindCertificate();
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by reading a certificate from a certificate blob.
		/// </summary>
		/// <param name="file">The contents of the certificate file.</param>
		/// <returns>A Certificate instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> if a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading the specified certificate.</exception>
		public static Certificate CreateFromCerFile(byte[] file) {
			if (file == null)
				throw new ArgumentNullException();
			return CreateFromCerFile(file, 0, file.Length);
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by reading a certificate from a certificate blob.
		/// </summary>
		/// <param name="file">The contents of the certificate file.</param>
		/// <param name="offset">The offset from which to start reading.</param>
		/// <param name="size">The length of the certificate.</param>
		/// <returns>A Certificate instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> if a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading the specified certificate.</exception>
		public static Certificate CreateFromCerFile(byte[] file, int offset, int size) {
			if (file == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset + size > file.Length)
				throw new ArgumentOutOfRangeException();
			IntPtr data = Marshal.AllocHGlobal(size);
			Marshal.Copy(file, offset, data, size);
			IntPtr handle = SspiProvider.CertCreateCertificateContext(SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, data, size);
			Marshal.FreeHGlobal(data);
			if (handle == IntPtr.Zero)
				throw new CertificateException("Unable to load the specified certificate.");
			else
				return new Certificate(handle);
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by duplicating an existing <see cref="X509Certificate"/> instance.
		/// </summary>
		/// <param name="certificate">The X509Certificate instance to duplicate.</param>
		/// <returns>A Certificate instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="certificate"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public static Certificate CreateFromX509Certificate(X509Certificate certificate) {
			if (certificate == null)
				throw new ArgumentNullException();
			return Certificate.CreateFromCerFile(certificate.GetRawCertData());
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by reading a certificate from a PEM encoded file.
		/// </summary>
		/// <param name="filename">The path to the PEM file.</param>
		/// <returns>A Certificate instance.</returns>
		/// <remarks>This implementation only reads certificates from PEM files. It does not read the private key from the certificate file, if one is present.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> if a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while reading from the file.</exception>
		/// <exception cref="CertificateException">An error occurs while reading the certificate from the PEM blob.</exception>
		public static Certificate CreateFromPemFile(string filename) {
			return CreateFromPemFile(CertificateStore.GetFileContents(filename));
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Certificate"/> class by reading a certificate from a PEM encoded file.
		/// </summary>
		/// <param name="file">The contents of the PEM file.</param>
		/// <returns>A Certificate instance.</returns>
		/// <remarks>This implementation only reads certificates from PEM files. It does not read the private key from the certificate file, if one is present.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="file"/> if a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while reading the certificate from the PEM blob.</exception>
		public static Certificate CreateFromPemFile(byte[] file) {
			if (file == null)
				throw new ArgumentNullException();
			string pemFile = Encoding.ASCII.GetString(file);
			string cert = GetCertString(pemFile, "CERTIFICATE");
			if (cert == null) {
				cert = GetCertString(pemFile, "X509 CERTIFICATE");
				if (cert == null)
					throw new CertificateException("The specified PEM file does not contain a certificate.");
			}
			byte[] certBuffer = Convert.FromBase64String(cert);
			return Certificate.CreateFromCerFile(certBuffer);
		}
		/// <summary>
		/// Extracts an encoded certificate from a PEM file.
		/// </summary>
		/// <param name="cert">The PEM encoded certificate file.</param>
		/// <param name="delimiter">The delimiter to search for.</param>
		/// <returns>The Base64 encoded certificate if successfull or a null reference otherwise.</returns>
		private static string GetCertString(string cert, string delimiter) {
			int start = cert.IndexOf("-----BEGIN " + delimiter + "-----");
			if (start < 0)
				return null;
			int end = cert.IndexOf("-----END " + delimiter + "-----", start);
			if (end < 0)
				return null;
			int sl = delimiter.Length + 16;
			int length = end - (start + sl);
			return cert.Substring(start + sl, length);
		}
		/// <summary>
		/// Duplicates a given certificate.
		/// </summary>
		/// <param name="certificate">The certificate to duplicate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="certificate"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public Certificate(Certificate certificate) {
			if (certificate == null)
				throw new ArgumentNullException();
			InitCertificate(certificate.Handle, true, null);
		}
		/// <summary>
		/// Initializes a new <see cref="Certificate"/> instance from a handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the state of the new instance.</param>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		public Certificate(IntPtr handle) : this(handle, false) {}
		/// <summary>
		/// Initializes a new <see cref="Certificate"/> instance from a handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the state of the new instance.</param>
		/// <param name="duplicate"><b>true</b> if the handle should be duplicated, <b>false</b> otherwise.</param>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		public Certificate(IntPtr handle, bool duplicate) {
			InitCertificate(handle, duplicate, null);
		}
		/// <summary>
		/// Initializes this <see cref="Certificate"/> instance from a handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the state of the new instance.</param>
		/// <param name="duplicate"><b>true</b> if the handle should be duplicated, <b>false</b> otherwise.</param>
		/// <param name="store">The store that owns the certificate.</param>
		/// <exception cref="ArgumentException"><paramref name="handle"/> is invalid.</exception>
		private void InitCertificate(IntPtr handle, bool duplicate, CertificateStore store) {
			if (handle == IntPtr.Zero)
				throw new ArgumentException("Invalid certificate handle!");
			if (duplicate)
				m_Handle = SspiProvider.CertDuplicateCertificateContext(handle);
			else
				m_Handle = handle;
			m_Context = (CertificateContext)Marshal.PtrToStructure(handle, typeof(CertificateContext));
			m_CertInfo = (CertificateInfo)Marshal.PtrToStructure(m_Context.pCertInfo, typeof(CertificateInfo));
			if (store == null) {
				m_Store = null;
			} else {
				m_Store = store;
			}
		}
		/// <summary>
		/// Returns the <see cref="CertificateInfo"/> structure associated with the certificate.
		/// </summary>
		/// <returns>A CertificateInfo instance.</returns>
		internal CertificateInfo GetCertificateInfo() {
			return m_CertInfo;
		}
		/// <summary>
		/// Initializes this <see cref="Certificate"/> instance from a handle.
		/// </summary>
		/// <param name="handle">The handle from which to initialize the state of the new instance.</param>
		/// <param name="store">The <see cref="CertificateStore"/> that contains the certificate.</param>
		internal Certificate(IntPtr handle, CertificateStore store) {
			InitCertificate(handle, false, store);
		}
		/// <summary>
		/// Creates a copy of this <see cref="Certificate"/>.
		/// </summary>
		/// <returns>The Certificate this method creates, cast as an object.</returns>
		public object Clone() {
			return new Certificate(SspiProvider.CertDuplicateCertificateContext(Handle));
		}
		/// <summary>
		/// Disposes of the certificate and frees unmanaged resources.
		/// </summary>
		~Certificate() {
			if (Handle != IntPtr.Zero) {
				SspiProvider.CertFreeCertificateContext(Handle);
				m_Handle = IntPtr.Zero;
			}
		}
		/// <summary>
		/// Returns a string representation of the current <see cref="Certificate"/> object.
		/// </summary>
		/// <returns>A string representation of the current Certificate object.</returns>
		public override string ToString() {
			return this.GetType().FullName;
		}
		/// <summary>
		/// Returns a string representation of the current X509Certificate object, with extra information, if specified.
		/// </summary>
		/// <param name="verbose"><b>true</b> to produce the verbose form of the string representation; otherwise, <b>false</b>.</param>
		/// <returns>A string representation of the current X509Certificate object.</returns>
		public string ToString(bool verbose) {
			if (verbose) {
				return "CERTIFICATE:\r\n" +
					"        Format:  X509\r\n" +
					"        Name:  " + GetName() + "\r\n" + 
					"        Issuing CA:  " + GetIssuerName() + "\r\n" +
					"        Key Algorithm:  " + GetKeyAlgorithm() + "\r\n" + 
					"        Serial Number:  " + GetSerialNumberString() + "\r\n" +
					"        Key Alogrithm Parameters:  " + GetKeyAlgorithmParametersString() + "\r\n" + 
					"        Public Key:  " + GetPublicKeyString();
			} else {
				return ToString();
			}
		}
		/// <summary>
		/// Returns the hash value for the X.509v3 certificate as an array of bytes.
		/// </summary>
		/// <returns>The hash value for the X.509 certificate.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the hash of the certificate.</exception>
		public byte[] GetCertHash() {
			return GetCertHash(HashType.Default);
		}
		/// <summary>
		/// Returns the hash value for the X.509v3 certificate as an array of bytes.
		/// </summary>
		/// <param name="type">One of the <see cref="HashType"/> values.</param>
		/// <returns>The hash value for the X.509 certificate.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the hash of the certificate.</exception>
		public byte[] GetCertHash(HashType type) {
			byte[] ret;
			IntPtr hash = Marshal.AllocHGlobal(256);
			try {
				int size = 256;
				if (SspiProvider.CertGetCertificateContextProperty(Handle, (int)type, hash, ref size) == 0 || size <= 0 || size > 256)
					throw new CertificateException("An error occurs while retrieving the hash of the certificate.");
				ret = new byte[size];
				Marshal.Copy(hash, ret, 0, size);
			} catch (Exception e) {
				throw e;
			} finally {
				Marshal.FreeHGlobal(hash);
			}
			return ret;
		}
		/// <summary>
		/// Returns the hash value for the X.509v3 certificate as a hexadecimal string.
		/// </summary>
		/// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the hash of the certificate.</exception>
		public string GetCertHashString() {
			return GetCertHashString(HashType.Default);
		}
		/// <summary>
		/// Returns the hash value for the X.509v3 certificate as a hexadecimal string.
		/// </summary>
		/// <param name="type">One of the <see cref="HashType"/> values.</param>
		/// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the hash of the certificate.</exception>
		public string GetCertHashString(HashType type) {
			return BytesToString(GetCertHash(type));
		}
		/// <summary>
		/// Converts an array of bytes to its hexadecimal string representation.
		/// </summary>
		/// <param name="buffer">The bytes to convert.</param>
		/// <returns>The hexadecimal representation of the byte array.</returns>
		private string BytesToString(byte[] buffer) {
			string ret = "";
			for(int i = 0; i < buffer.Length; i++) {
				ret += buffer[i].ToString("X2");
			}
			return ret;
		}
		/// <summary>
		/// Returns the effective date of this X.509v3 certificate.
		/// </summary>
		/// <returns>The effective date for this X.509 certificate.</returns>
		/// <remarks>The effective date is the date after which the X.509 certificate is considered valid.</remarks>
		public DateTime GetEffectiveDate() {
			return DateTime.FromFileTime(m_CertInfo.NotBefore);
		}
		/// <summary>
		/// Returns the expiration date of this X.509v3 certificate.
		/// </summary>
		/// <returns>The expiration date for this X.509 certificate.</returns>
		/// <remarks>The expiration date is the date after which the X.509 certificate is no longer considered valid.</remarks>
		public DateTime GetExpirationDate() {
			return DateTime.FromFileTime(m_CertInfo.NotAfter);
		}
		/// <summary>
		/// Returns the name of the certification authority that issued the X.509v3 certificate.
		/// </summary>
		/// <returns>The name of the certification authority that issued the X.509 certificate.</returns>
		// Thanks go out to Hernan de Lahitte for fixing a bug in this method.
		public string GetIssuerName() {
			int length = SspiProvider.CertGetNameString(Handle, SecurityConstants.CERT_NAME_SIMPLE_DISPLAY_TYPE, SecurityConstants.CERT_NAME_DISABLE_IE4_UTF8_FLAG | SecurityConstants.CERT_NAME_ISSUER_FLAG, IntPtr.Zero, IntPtr.Zero, 0);
			if (length <= 0)
				throw new CertificateException("An error occurs while requesting the issuer name.");
			IntPtr name = Marshal.AllocHGlobal(length);
			SspiProvider.CertGetNameString(Handle, SecurityConstants.CERT_NAME_SIMPLE_DISPLAY_TYPE, SecurityConstants.CERT_NAME_DISABLE_IE4_UTF8_FLAG | SecurityConstants.CERT_NAME_ISSUER_FLAG, IntPtr.Zero, name, length);
			string ret = Marshal.PtrToStringAnsi(name);
			Marshal.FreeHGlobal(name);
			return ret;
		}
		/// <summary>
		/// Returns the key algorithm information for this X.509v3 certificate.
		/// </summary>
		/// <returns>The key algorithm information for this X.509 certificate as a string.</returns>
		public string GetKeyAlgorithm() {
			return Marshal.PtrToStringAnsi(m_CertInfo.SignatureAlgorithmpszObjId);
		}
		/// <summary>
		/// Returns the key algorithm parameters for the X.509v3 certificate.
		/// </summary>
		/// <returns>The key algorithm parameters for the X.509 certificate as an array of bytes.</returns>
		public byte[] GetKeyAlgorithmParameters() {
			byte[] ret = new byte[m_CertInfo.SignatureAlgorithmParameterscbData];
			if (ret.Length > 0)
				Marshal.Copy(m_CertInfo.SignatureAlgorithmParameterspbData, ret, 0, ret.Length);
			return ret;
		}
		/// <summary>
		/// Returns the key algorithm parameters for the X.509v3 certificate.
		/// </summary>
		/// <returns>The key algorithm parameters for the X.509 certificate as a hexadecimal string.</returns>
		public string GetKeyAlgorithmParametersString() {
			return BytesToString(GetKeyAlgorithmParameters());
		}
		/// <summary>
		/// Returns the public key for the X.509v3 certificate.
		/// </summary>
		/// <returns>The public key for the X.509 certificate as an array of bytes.</returns>
		public byte[] GetPublicKey() {
			byte[] key = new byte[m_CertInfo.SubjectPublicKeyInfoPublicKeycbData];
			Marshal.Copy(m_CertInfo.SubjectPublicKeyInfoPublicKeypbData, key, 0, key.Length);
			return key;
		}
		/// <summary>
		/// Returns the public key for the X.509v3 certificate.
		/// </summary>
		/// <returns>The public key for the X.509 certificate as a hexadecimal string.</returns>
		public string GetPublicKeyString() {
			return BytesToString(GetPublicKey());
		}
		/// <summary>
		/// Returns the raw data for the entire X.509v3 certificate.
		/// </summary>
		/// <returns>A byte array containing the X.509 certificate data.</returns>
		public byte[] GetRawCertData() {
			byte[] ret = new byte[m_Context.cbCertEncoded];
			Marshal.Copy(m_Context.pbCertEncoded, ret, 0, ret.Length);
			return ret;
		}
		/// <summary>
		/// Returns the raw data for the entire X.509v3 certificate.
		/// </summary>
		/// <returns>The X.509 certificate data as a hexadecimal string.</returns>
		public string GetRawCertDataString() {
			return BytesToString(GetRawCertData());
		}
		/// <summary>
		/// Returns the serial number of the X.509v3 certificate.
		/// </summary>
		/// <returns>The serial number of the X.509 certificate as an array of bytes.</returns>
		public byte[] GetSerialNumber() {
			byte[] ret = new byte[m_CertInfo.SerialNumbercbData];
			if (ret.Length > 0) {
				Marshal.Copy(m_CertInfo.SerialNumberpbData, ret, 0, ret.Length);
				Array.Reverse(ret);
			}
			return ret;
		}
		/// <summary>
		/// Returns the serial number of the X.509v3 certificate.
		/// </summary>
		/// <returns>The serial number of the X.509 certificate as a hexadecimal string.</returns>
		public string GetSerialNumberString() {
			return BytesToString(GetSerialNumber());
		}
		/// <summary>
		/// Returns the length of the public key of the X.509v3 certificate.
		/// </summary>
		/// <returns>Returns the length of the public key in bits. If unable to determine the key's length, returns zero.</returns>
		public int GetPublicKeyLength() {
			return SspiProvider.CertGetPublicKeyLength(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, new IntPtr(m_Context.pCertInfo.ToInt64() + 56));
		}
		/// <summary>
		/// Returns a list of attributes of the X.509v3 certificate.
		/// </summary>
		/// <returns>A StringDictionary that contains the attributes.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the attributes.</exception>
		public DistinguishedName GetDistinguishedName() {
			return new DistinguishedName(m_CertInfo.SubjectpbData, m_CertInfo.SubjectcbData);
		}
		/// <summary>
		/// Returns a list of extensions of the X.509v3 certificate.
		/// </summary>
		/// <returns>An array of Extension instances.</returns>
		public Extension[] GetExtensions() {
			Extension[] ret = new Extension[m_CertInfo.cExtension];
			int extSize = 8 + IntPtr.Size * 2;
			IntPtr ptr = m_CertInfo.rgExtension;
			Type cest = typeof(CERT_EXTENSION);
			CERT_EXTENSION ce;
			for(int i = 0;  i < m_CertInfo.cExtension;  i++) {
				ce = (CERT_EXTENSION)Marshal.PtrToStructure(ptr, cest);
				ret[i] = new Extension(Marshal.PtrToStringAnsi(ce.pszObjId), ce.fCritical != 0, new byte[ce.ValuecbData]);
				Marshal.Copy(ce.ValuepbData, ret[i].EncodedValue, 0, ce.ValuecbData);
				ptr = new IntPtr(ptr.ToInt64() + extSize);
			}
			return ret;
		}
		/// <summary>
		/// Searches for a certificate extension.
		/// </summary>
		/// <param name="oid">The extension to search for.</param>
		/// <returns>An instance of the <see cref="Extension"/> class -or- a null reference (<b>Nothing</b> in Visual Basic) if the specified extension could not be found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="oid"/> is a null  reference (<b>Nothing</b> in Visual Basic).</exception>
		public Extension FindExtension(string oid) {
			if (oid == null)
				throw new ArgumentNullException();
			IntPtr ret = SspiProvider.CertFindExtension(oid, m_CertInfo.cExtension, m_CertInfo.rgExtension);
			if (ret == IntPtr.Zero) {
				return null;
			} else {
				CERT_EXTENSION ce = (CERT_EXTENSION)Marshal.PtrToStructure(ret, typeof(CERT_EXTENSION));
				Extension ext = new Extension(Marshal.PtrToStringAnsi(ce.pszObjId), ce.fCritical != 0, new byte[ce.ValuecbData]);
				Marshal.Copy(ce.ValuepbData, ext.EncodedValue, 0, ce.ValuecbData);
				return ext;
			}
		}
		/// <summary>
		/// Decodes the specified extension and returns an object of the specified type that is instantiated with the decoded bytes.
		/// </summary>
		/// <param name="extension">The certificate extension to decode.</param>
		/// <param name="oid">One of the predefined constants specified in the Win32 CryptoAPI. Refer to the documentation of the <a href="http://msdn.microsoft.com/library/en-us/security/security/cryptdecodeobject.asp">CryptDecodeObject</a> function for more information.</param>
		/// <param name="returnType">A <see cref="Type"/> instance. See remarks.</param>
		/// <returns>An object of the type <paramref name="returnType"/>.</returns>
		/// <remarks><p>
		/// The specified type should have a public constructor that takes an IntPtr and an int as parameters [in that order].
		/// The IntPtr is a pointer to the decoded buffer and the int contains the number of decoded bytes.
		/// The type should not keep the IntPtr reference after construction of an instance, because the memory is freed when the DecodeExtension method returns.
		/// </p></remarks>
		/// <exception cref="ArgumentNullException">One of the parameters is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while decoding the certificate extension.</exception>
		public static object DecodeExtension(Extension extension, int oid, Type returnType) {
			return DecodeExtension(extension, new IntPtr(oid), returnType);
		}
		/// <summary>
		/// Decodes the specified extension and returns an object of the specified type that is instantiated with the decoded bytes.
		/// </summary>
		/// <param name="extension">The certificate extension to decode.</param>
		/// <param name="oid">The Object Identifier of the structure. Refer to the documentation of the <a href="http://msdn.microsoft.com/library/en-us/security/security/cryptdecodeobject.asp">CryptDecodeObject</a> function for more information.</param>
		/// <param name="returnType">A <see cref="Type"/> instance. See remarks.</param>
		/// <returns>An object of the type <paramref name="returnType"/>.</returns>
		/// <remarks><p>
		/// The specified type should have a public constructor that takes an IntPtr and an int as parameters [in that order].
		/// The IntPtr is a pointer to the decoded buffer and the int contains the number of decoded bytes.
		/// The type should not keep the IntPtr reference after construction of an instance, because the memory is freed when the DecodeExtension method returns.
		/// </p></remarks>
		/// <exception cref="ArgumentNullException">One of the parameters is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while decoding the certificate extension.</exception>
		public static object DecodeExtension(Extension extension, string oid, Type returnType) {
			if (oid == null)
				throw new ArgumentNullException("oid");
			IntPtr buffer = Marshal.StringToHGlobalAnsi(oid);
			try {
				return DecodeExtension(extension, buffer, returnType);
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}
		/// <summary>
		/// Decodes the specified extension and returns an object of the specified type that is instantiated with the decoded bytes.
		/// </summary>
		/// <param name="extension">The certificate extension to decode.</param>
		/// <param name="oid">The Object Identifier of the structure.</param>
		/// <param name="returnType">A <see cref="Type"/> instance. See remarks.</param>
		/// <returns>An object of the type <paramref name="returnType"/>.</returns>
		/// <remarks><p>
		/// The specified type should have a public constructor that takes an IntPtr and an int as parameters [in that order].
		/// The IntPtr is a pointer to the decoded buffer and the int contains the number of decoded bytes.
		/// The type should not keep the IntPtr reference after construction of an instance, because the memory is freed when the DecodeExtension method returns.
		/// </p></remarks>
		/// <exception cref="ArgumentNullException">One of the parameters is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while decoding the certificate extension.</exception>
		protected static object DecodeExtension(Extension extension, IntPtr oid, Type returnType) {
			if (extension.EncodedValue == null || returnType == null)
				throw new ArgumentNullException();
			int size = 0;
			if (SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, oid, extension.EncodedValue, extension.EncodedValue.Length, 0, IntPtr.Zero, ref size) == 0)
				throw new CertificateException("Could not decode the extension.");
			IntPtr buffer = Marshal.AllocHGlobal(size);
			try {
				if (SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, oid, extension.EncodedValue, extension.EncodedValue.Length, 0, buffer, ref size) == 0)
					throw new CertificateException("Could not decode the extension.");
				return Activator.CreateInstance(returnType, new object[] {buffer, size});
			} catch (CertificateException ce) {
				throw ce;
			} catch (Exception e) {
				throw new CertificateException("Unable to instantiate the specified object type.", e);
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}
		/// <summary>
		/// Returns the name of the current principal.
		/// </summary>
		/// <returns>The name of the current principal.</returns>
		/// <exception cref="CertificateException">The certificate does not have a name attribute.</exception>
		// Thanks go out to Jonni Faiga for notifying us about a bug in this method.
		public string GetName() {
			int size = 0;
			SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, new IntPtr(SecurityConstants.X509_UNICODE_NAME), m_CertInfo.SubjectpbData, m_CertInfo.SubjectcbData, 0, IntPtr.Zero, ref size);
			if (size <= 0)
				throw new CertificateException("Unable to decode the name of the certificate.");
			IntPtr buffer = IntPtr.Zero;
			string ret = null;
			try {
				buffer = Marshal.AllocHGlobal(size);
				if (SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, new IntPtr(SecurityConstants.X509_UNICODE_NAME), m_CertInfo.SubjectpbData, m_CertInfo.SubjectcbData, 0, buffer, ref size) == 0)
					throw new CertificateException("Unable to decode the name of the certificate.");
				IntPtr attPointer = SspiProvider.CertFindRDNAttr(SecurityConstants.szOID_COMMON_NAME, buffer);
				if (attPointer == IntPtr.Zero)
					attPointer = SspiProvider.CertFindRDNAttr(SecurityConstants.szOID_RSA_unstructName, buffer);
				if (attPointer == IntPtr.Zero)
					attPointer = SspiProvider.CertFindRDNAttr(SecurityConstants.szOID_ORGANIZATION_NAME, buffer);
				if (attPointer != IntPtr.Zero) {
					RdnAttribute att = (RdnAttribute)Marshal.PtrToStructure(attPointer, typeof(RdnAttribute));
					ret = Marshal.PtrToStringUni(att.pbData, att.cbData / 2);
				}
			} catch (CertificateException ce) {
				throw ce;
			} catch (Exception e) {
				throw new CertificateException("Could not get certificate attributes.", e);
			} finally {
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
			if (ret == null)
				throw new CertificateException("Certificate does not have a name attribute.");
			else
				return ret;
		}
		/// <summary>
		/// Returns a list of intended key usages of the X.509v3 certificate.
		/// </summary>
		/// <returns>An integer that contains a list of intended key usages.</returns>
		/// <remarks>Use the bitwise And operator to check whether a specific key usage is set.</remarks>
		public int GetIntendedKeyUsage() { // returns one or more of the KeyUsage values
			IntPtr buffer = Marshal.AllocHGlobal(4);
			SspiProvider.CertGetIntendedKeyUsage(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, m_Context.pCertInfo, buffer, 4);
			byte[] mb = new byte[4];
			Marshal.Copy(buffer, mb, 0, 4);
			Marshal.FreeHGlobal(buffer);
			return BitConverter.ToInt32(mb, 0);
		}
		/// <summary>
		/// Returns a list of enhanced key usages of the X.509v3 certificate.
		/// </summary>
		/// <returns>A StringCollection that contains a list of the enhanced key usages.</returns>
		/// <exception cref="CertificateException">An error occurs while retrieving the enhanced key usages.</exception>
		public StringCollection GetEnhancedKeyUsage() {
			StringCollection ret = new StringCollection();
			int size = 0;
			SspiProvider.CertGetEnhancedKeyUsage(Handle, 0, IntPtr.Zero, ref size); 
			if (size <= 0)
				return ret;
			IntPtr buffer = Marshal.AllocHGlobal(size);
			try {
				if (SspiProvider.CertGetEnhancedKeyUsage(Handle, 0, buffer, ref size) == 0)
					throw new CertificateException("Could not obtain the enhanced key usage.");
				TrustListUsage cu = (TrustListUsage)Marshal.PtrToStructure(buffer, typeof(TrustListUsage));
				for(int i = 0; i < cu.cUsageIdentifier; i++) {
					IntPtr ip = Marshal.ReadIntPtr(cu.rgpszUsageIdentifier, i * IntPtr.Size);
					try {
						ret.Add(Marshal.PtrToStringAnsi(ip));
					} catch {}
				}
				return ret;
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}
		/// <summary>
		/// Returns a <see cref="CertificateChain"/> where the leaf certificate corresponds to this <see cref="Certificate"/>.
		/// </summary>
		/// <returns>The CertificateChain corresponding to this Certificate.</returns>
		/// <exception cref="CertificateException">An error occurs while building the certificate chain.</exception>
		public CertificateChain GetCertificateChain() {
			if (m_Chain == null)
				m_Chain = new CertificateChain(this, Store);
			return m_Chain;
		}
		/// <summary>
		/// Checks whether the <see cref="Certificate"/> has a private key associated with it.
		/// </summary>
		/// <returns><b>true</b> if the certificate has a private key associated with it, <b>false</b> otherwise.</returns>
		public bool HasPrivateKey() {
			int handle = 0;
			int keyspec = 0;
			int free = 0;
			bool ret = false;
			if (SspiProvider.CryptAcquireCertificatePrivateKey(Handle, SecurityConstants.CRYPT_ACQUIRE_COMPARE_KEY_FLAG | SecurityConstants.CRYPT_ACQUIRE_SILENT_FLAG, IntPtr.Zero, ref handle, ref keyspec, ref free) != 0) {
				ret = true;
			}
			if (free != 0)
				SspiProvider.CryptReleaseContext(handle, 0);
			return ret;
		}
		/// <summary>
		/// Returns the name of the format of this X.509v3 certificate.
		/// </summary>
		/// <returns>The format of this X.509 certificate.</returns>
		/// <remarks>The format X.509 is always returned in this implementation.</remarks>
		public string GetFormat() {
			return "X509";
		}
		/// <summary>
		/// Returns the hash code for the X.509v3 certificate as an integer.
		/// </summary>
		/// <returns>The hash code for the X.509 certificate as an integer.</returns>
		/// <remarks>If the X.509 certificate hash is an array of more than 4 bytes, any byte after the fourth byte is not seen in this integer representation.</remarks>
		public override int GetHashCode() {
			byte[] hash = GetCertHash();
			byte[] buffer = new byte[4];
			if (hash.Length < buffer.Length)
				Array.Copy(hash, 0, buffer, 0, hash.Length);
			else
				Array.Copy(hash, 0, buffer, 0, buffer.Length);
			return BitConverter.ToInt32(buffer, 0);
		}
		/// <summary>
		/// Compares two <see cref="Certificate"/> objects for equality.
		/// </summary>
		/// <param name="other">A Certificate object to compare to the current object.</param>
		/// <returns><b>true</b> if the current Certificate object is equal to the object specified by <paramref name="other"/>; otherwise, <b>false</b>.</returns>
		public virtual bool Equals(Certificate other) {
			if (other == null)
				return false;
			return SspiProvider.CertCompareCertificate(SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, m_Context.pCertInfo, other.m_Context.pCertInfo) != 0;
		}
		/// <summary>
		/// Compares a <see cref="Certificate"/> object and an <see cref="X509Certificate"/> object for equality.
		/// </summary>
		/// <param name="other">An X509Certificate object to compare to the current object.</param>
		/// <returns><b>true</b> if the current Certificate object is equal to the object specified by <paramref name="other"/>; otherwise, <b>false</b>.</returns>
		public virtual bool Equals(X509Certificate other) {
			if (other == null)
				return false;
			return other.GetCertHashString() == this.GetCertHashString();
		}
		/// <summary>
		/// Compares two <see cref="Certificate"/> objects for equality.
		/// </summary>
		/// <param name="other">A Certificate object to compare to the current object.</param>
		/// <returns><b>true</b> if the current Certificate object is equal to the object specified by <paramref name="other"/>; otherwise, <b>false</b>.</returns>
		public override bool Equals(object other) {
			try {
				return Equals((Certificate)other);
			} catch {
				try {
					return Equals((X509Certificate)other);
				} catch {
					return false;
				}
			}
		}
		/// <summary>
		/// Returns an array of usages consisting of the intersection of the valid usages for all certificates in an array of certificates.
		/// </summary>
		/// <param name="certificates">Array of certificates to be checked for valid usage.</param>
		/// <returns>An array of valid usages -or- a null reference (<b>Nothing</b> in Visual Basic) if all certificates support all usages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="certificates"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The array of certificates contains at least one invalid entry.</exception>
		/// <exception cref="CertificateException">An error occurs while determining the intersection of valid usages.</exception>
		public static string[] GetValidUsages(Certificate[] certificates) {
			if (certificates == null)
				throw new ArgumentNullException();
			IntPtr buffer = IntPtr.Zero;
			IntPtr certs = Marshal.AllocHGlobal(certificates.Length * IntPtr.Size);
			try {
				for(int i = 0; i < certificates.Length; i++) {
					if (certificates[i] == null)
						throw new ArgumentException();
					Marshal.WriteIntPtr(certs, i * IntPtr.Size, certificates[i].Handle);
				}
				int count = 0, bytes = 0;
				if (SspiProvider.CertGetValidUsages(certificates.Length, certs, ref count, buffer, ref bytes) == 0)
					throw new CertificateException("Unable to get the valid usages.");
				if (count == -1)
					return null; // every usage is valid
				buffer = Marshal.AllocHGlobal(bytes);
				if (SspiProvider.CertGetValidUsages(certificates.Length, certs, ref count, buffer, ref bytes) == 0)
					throw new CertificateException("Unable to get the valid usages.");
				string[] ret = new string[count];
				for(int i = 0; i < count; i++) {
					ret[i] = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(buffer, i * IntPtr.Size));
				}
				return ret;
			} finally {
				Marshal.FreeHGlobal(certs);
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}
		/// <summary>
		/// Saves the <see cref="Certificate"/> as a PFX encoded file.
		/// </summary>
		/// <param name="filename">The filename of the new PFX file.</param>
		/// <param name="password">The password to use when encrypting the private keys.</param>
		/// <param name="withPrivateKeys"><b>true</b> if the private keys should be exported [if possible], <b>false</b> otherwise.</param>
		/// <param name="withParents"><b>true</b> if the parent certificates should be exported too [if possible], <b>false</b> otherwise.</param>
		/// <remarks>If the specified file already exists, the method will throw an exception.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> or <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while writing the data to the file.</exception>
		/// <exception cref="CertificateException">An error occurs while exporting the certificate store<br>-or-</br><br>an error occurs while building the certificate chain</br><br>-or-</br><br>an error occurs while creating the store</br><br>-or-</br><br>an error occurs while adding the certificate to the store.</br></exception>
		public void ToPfxFile(string filename, string password, bool withPrivateKeys, bool withParents) {
			CreateCertStore(withParents).ToPfxFile(filename, password, withPrivateKeys);
		}
		/// <summary>
		/// Saves the <see cref="Certificate"/> as a PFX encoded buffer.
		/// </summary>
		/// <param name="password">The password to use when encrypting the private keys.</param>
		/// <param name="withPrivateKeys"><b>true</b> if the private keys should be exported [if possible], <b>false</b> otherwise.</param>
		/// <param name="withParents"><b>true</b> if the parent certificates should be exported too [if possible], <b>false</b> otherwise.</param>
		/// <returns>An array of bytes that represents the PFX encoded certificate.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while exporting the certificate store<br>-or-</br><br>an error occurs while building the certificate chain</br><br>-or-</br><br>an error occurs while creating the store</br><br>-or-</br><br>an error occurs while adding the certificate to the store.</br></exception>
		public byte[] ToPfxBuffer(string password, bool withPrivateKeys, bool withParents) {
			return CreateCertStore(withParents).ToPfxBuffer(password, withPrivateKeys);
		}
		/// <summary>
		/// Creates an in memory <see cref="CertificateStore"/> with this <see cref="Certificate"/> in it.
		/// </summary>
		/// <param name="withParents"><b>true</b> if the parent certificates should be included [if possible], <b>false</b> otherwise.</param>
		/// <returns>A CertificateStore instance.</returns>
		private CertificateStore CreateCertStore(bool withParents) {
			CertificateStore store = new CertificateStore();
			if (withParents) {
				Certificate[] c = this.GetCertificateChain().GetCertificates();
				for(int i = 0; i < c.Length; i++) {
					store.AddCertificate(c[i]);
				}
			} else {
				store.AddCertificate(this);
			}
			return store;
		}
		/// <summary>
		/// Saves the <see cref="Certificate"/> as an encoded file.
		/// </summary>
		/// <param name="filename">The file where to store the certificate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while writing the data.</exception>
		/// <remarks>If the specified file already exists, this method will throw an exception.</remarks>
		public void ToCerFile(string filename) {
			SaveToFile(GetCertificateBuffer(), filename);
		}
		/// <summary>
		/// Saves the <see cref="Certificate"/> as an encoded buffer.
		/// </summary>
		/// <returns>An array of bytes that represents the encoded certificate.</returns>
		public byte[] ToCerBuffer() {
			return GetCertificateBuffer();
		}
		/// <summary>
		/// Returns a buffer with the encoded certificate.
		/// </summary>
		/// <returns>An array of bytes.</returns>
		private byte[] GetCertificateBuffer() {
			byte[] buffer = new byte[m_Context.cbCertEncoded];
			Marshal.Copy(m_Context.pbCertEncoded, buffer, 0, m_Context.cbCertEncoded);
			return buffer;
		}
		/// <summary>
		/// Writes a buffer with data to a file.
		/// </summary>
		/// <param name="buffer">The buffer to write.</param>
		/// <param name="filename">The filename to write the data to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filename"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="IOException">An error occurs while writing the data.</exception>
		private void SaveToFile(byte[] buffer, string filename) {
			if (filename == null)
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
		/// Returns an X509Certificate object that corresponds to this <see cref="Certificate"/>.
		/// </summary>
		/// <returns>An X509Certificate instance.</returns>
		public X509Certificate ToX509() {
			return new X509Certificate(SspiProvider.CertDuplicateCertificateContext(Handle));
		}
		/// <summary>
		/// Gets the handle of the Certificate.
		/// </summary>
		/// <value>An IntPtr that represents the handle of the certificate.</value>
		/// <remarks>The handle returned by this property should not be closed. If the handle is closed by an external actor, the methods of the Certificate object may fail in undocumented ways [for instance, an Access Violation may occur].</remarks>
		public IntPtr Handle {
			get {
				return m_Handle;
			}
		}
		/// <summary>
		/// Duplicates the handle of the Certificate.
		/// </summary>
		/// <returns>A duplicate handle of the Certificate.</returns>
		internal IntPtr DuplicateHandle() {
			return SspiProvider.CertDuplicateCertificateContext(this.Handle);
		}
		/// <summary>
		/// Gets the handle of the associated <see cref="CertificateStore"/>, if any.
		/// </summary>
		/// <value>A CertificateStore instance -or- a null reference (<b>Nothing</b> in Visual Basic) is no store is associated with this certificate.</value>
		internal CertificateStore Store {
			get {
				return m_Store;
			}
			set {
				if (m_Store != value)
					m_Chain = null; // force a recreation of the certificate chain, if necessary
				m_Store = value;
			}
		}
		/// <summary>
		/// Gets a value indicating whether the certificate is current, that is, has not expired.
		/// </summary>
		/// <value><b>true</b> if the certificate is current; otherwise, <b>false</b>.</value>
		public bool IsCurrent {
			get {
				return SspiProvider.CertVerifyTimeValidity(IntPtr.Zero, m_Context.pCertInfo) == 0;
			}
		}
		/// <summary>
		/// Gets a value indicating whether the certificate can be used for encrypting and decrypting messages.
		/// </summary>
		/// <value><b>true</b> if the certificate can be used for data encryption; otherwise, <b>false</b>.</value>
		public bool SupportsDataEncryption {
			get {
				return this.GetIntendedKeyUsage()==0 || ((this.GetIntendedKeyUsage() & SecurityConstants.CERT_DATA_ENCIPHERMENT_KEY_USAGE) != 0);
			}
		}
		/// <summary>
		/// Gets a value indicating whether the certificate can be used for digital signatures.
		/// </summary>
		/// <value><b>true</b> if the certificate can be used for digital signature; otherwise, <b>false</b>.</value>
		public bool SupportsDigitalSignature {
			get {
				return this.GetIntendedKeyUsage()==0 || ((this.GetIntendedKeyUsage() & SecurityConstants.CERT_DIGITAL_SIGNATURE_KEY_USAGE) != 0);
			}
		}
		/// <summary>
		/// Creates a new Certificate from a string representation.
		/// </summary>
		/// <param name="rawString">A Base64-encoded representation of the certificate.</param>
		/// <returns>A new Certificate.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="rawString"/> if a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while loading the specified certificate.</exception>
		/// <exception cref="FormatException">The length of <paramref name="rawString"/> is less than 4 -or- the length of <paramref name="rawString"/> is not an even multiple of 4.</exception>
		public static Certificate CreateFromBase64String(string rawString) {
			if (rawString == null)
				throw new ArgumentNullException("rawString");
			return Certificate.CreateFromCerFile(Convert.FromBase64String(rawString));
		}
		/// <summary>
		/// Returns a Base64-encoded representation of the certificate.
		/// </summary>
		/// <returns>A Base64-encoded representation of the certificate.</returns>
		public string ToBase64String() {
			byte[] cert = this.ToCerBuffer();
			return Convert.ToBase64String(cert, 0, cert.Length);
		}
		/// <summary>
		/// Converts the <see cref="Certificate"/> to a PEM encoded buffer.
		/// </summary>
		/// <returns>An array of bytes that represents the PEM encoded certificate.</returns>
		public byte[] ToPemBuffer() {
			return Encoding.ASCII.GetBytes("-----BEGIN CERTIFICATE-----\r\n" + ToBase64String() + "\r\n-----END CERTIFICATE-----\r\n");
		}
		/// <summary>
		/// Gets the unique identifier associated with the key.
		/// </summary>
		/// <returns>A byte array containing the unique identifier associated with the key.</returns>
		public byte[] GetKeyIdentifier() {
			int size = 0;
			SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_IDENTIFIER_PROP_ID, null, ref size);
			byte[] ret = new byte[size];
			SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_IDENTIFIER_PROP_ID, ret, ref size);
			return ret;
		}
		/// <summary>
		/// Gets the private key for the certificate.
		/// </summary>
		/// <value>A System.Security.Cryptography.RSA containing the private key for the certificate.</value>
		/// <exception cref="CertificateException">An error occurs while retrieving the RSA instance associated with the certificate.</exception>
		// Thanks go out to Hernan de Lahitte for fixing a bug in this method
		public RSA PrivateKey {
			get {
				//int ku = this.GetIntendedKeyUsage();
				//if (!((ku & (int)KeyUsage.DataEncipherment) != 0 || (ku & (int)KeyUsage.KeyEncipherment) != 0))
				//	throw new CertificateException("This certificate is not suited for data encipherment or key encipherment.");
				int flags = 0, provider = 0, keySpec = 0, mustFree = 0;
				if (!Environment.UserInteractive)
					flags = SecurityConstants.CRYPT_ACQUIRE_SILENT_FLAG;
				if (SspiProvider.CryptAcquireCertificatePrivateKey(this.Handle, flags, IntPtr.Zero, ref provider, ref keySpec, ref mustFree) == 0)
					throw new CertificateException("Could not acquire private key.");
				if (mustFree != 0)
					SspiProvider.CryptReleaseContext(provider, 0); // we don't need it in the rest of our code
				if (Environment.UserInteractive) {
					flags = 0;
				} else {
					flags = SecurityConstants.CRYPT_FIND_SILENT_KEYSET_FLAG;
				}
				int length = 0;
				if (SspiProvider.CryptFindCertificateKeyProvInfo(this.Handle, flags, IntPtr.Zero) == 0
					|| SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_PROV_INFO_PROP_ID, null, ref length) == 0)
					throw new CertificateException("Could not query the associated private key.");
				IntPtr buffer = Marshal.AllocHGlobal(length);
				RSA privateKey = null;
				try {
					SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_PROV_INFO_PROP_ID, buffer, ref length);
					CRYPT_KEY_PROV_INFO provInfo = (CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(buffer, typeof(CRYPT_KEY_PROV_INFO));
					CspParameters cspParams = new CspParameters();
					cspParams.KeyContainerName = provInfo.pwszContainerName;
					cspParams.ProviderName = null; //provInfo.pwszProvName;
					cspParams.ProviderType = provInfo.dwProvType;
					cspParams.KeyNumber = provInfo.dwKeySpec;
					if((provInfo.dwFlags & 32) != 0)
						cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
					privateKey = new RSACryptoServiceProvider(cspParams);
				} catch (CertificateException e) {
					throw e;
				} catch (Exception e) {
					throw new CertificateException("An error occurs while accessing the certificate's private key.", e);
				} finally {
					Marshal.FreeHGlobal(buffer);
				}
				return privateKey;
			}
		}
		/// <summary>
		/// Gets the public key derived from the certificate's data. This key cannot be used to sign or decrypt data.
		/// </summary>
		/// <value>A System.Security.Cryptography.RSA that contains the public key derived from the certificate's data.</value>
		/// <exception cref="CertificateException">An error occurs while retrieving the RSA instance associated with the certificate.</exception>
		public RSA PublicKey {
			get{
				//int ku = this.GetIntendedKeyUsage();
				//if (!((ku & (int)KeyUsage.DataEncipherment) != 0 || (ku & (int)KeyUsage.KeyEncipherment) != 0))
				//	throw new CertificateException("This certificate is not suited for data encipherment or key encipherment.");
				IntPtr buffer = IntPtr.Zero;
				int provider = CAPIProvider.ContainerHandle, key = 0;
				RSA publicKey = null;
				try {
/*					int flags = 0;
					if (!Environment.UserInteractive && Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5)
						flags = SecurityConstants.CRYPT_SILENT;*/
/*					if (SspiProvider.CryptAcquireContext(ref provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, flags) == 0) {
						if (SspiProvider.CryptAcquireContext(ref provider, IntPtr.Zero, null, SecurityConstants.PROV_RSA_FULL, flags | SecurityConstants.CRYPT_NEWKEYSET) == 0) {
							throw new CertificateException("Could not acquire crypto context.");
						}
					}*/
					CERT_PUBLIC_KEY_INFO pki = new CERT_PUBLIC_KEY_INFO(m_CertInfo);
					int size = 0;
					if (SspiProvider.CryptImportPublicKeyInfoEx(provider, SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, ref pki, 0, 0, IntPtr.Zero, ref key) == 0)
						throw new CertificateException("Could not obtain the handle of the public key.");
					if (SspiProvider.CryptExportKey(key, 0, SecurityConstants.PUBLICKEYBLOB, 0, IntPtr.Zero, ref size) == 0)
						throw new CertificateException("Could not get the size of the key.");
					buffer = Marshal.AllocHGlobal(size);
					if (SspiProvider.CryptExportKey(key, 0, SecurityConstants.PUBLICKEYBLOB, 0, buffer, ref size) == 0)
						throw new CertificateException("Could not export the key.");
					PUBLIC_KEY_BLOB pkb = (PUBLIC_KEY_BLOB)Marshal.PtrToStructure(buffer, typeof(PUBLIC_KEY_BLOB));
					if (pkb.magic != 0x31415352) // make sure we're dealing with an RSA public key
						throw new CertificateException("This is not an RSA certificate.");
					// initialize the RSAParameters structure
					RSAParameters rsap = new RSAParameters();
					rsap.Exponent = ConvertIntToByteArray(pkb.pubexp);
					IntPtr modulus = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(PUBLIC_KEY_BLOB)));
					rsap.Modulus = new byte[pkb.bitlen / 8];
					Marshal.Copy(modulus, rsap.Modulus, 0, rsap.Modulus.Length);
					Array.Reverse(rsap.Modulus); // key is stored in reversed order
					// initialize the CspParameters structure
					CspParameters cp = new CspParameters();
					cp.Flags = CspProviderFlags.UseMachineKeyStore;
					// create the RSA object
					publicKey = new RSACryptoServiceProvider(cp);
					publicKey.ImportParameters(rsap);
				} finally {
					if (key != 0)
						SspiProvider.CryptDestroyKey(key);
/*					if (provider != 0)
						SspiProvider.CryptReleaseContext(provider, 0);*/
					if (buffer != IntPtr.Zero)
						Marshal.FreeHGlobal(buffer);
				}
				return publicKey;
			}
		}
		/*		WARNING: following code doesn't always work [for instance during the SSL/TLS handshake]
						 and is also much slower compared to the above method.

				/// <summary>
				/// Returns a CspParameters instance that contains the information of the public key of this certificate.
				/// </summary>
				/// <returns>A <see cref="CspParameters"/> instance.</returns>
				// Thanks go out to Hernan de Lahitte for implementing this method.
				private CspParameters GetCspParameters() {
					int length = 0;
					int flags = (Environment.UserInteractive) ? 0 : SecurityConstants.CRYPT_FIND_SILENT_KEYSET_FLAG;
					//SspiProvider.CryptFindCertificateKeyProvInfo(this.Handle, flags, IntPtr.Zero) == 0 ||
					if (SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_PROV_INFO_PROP_ID, null, ref length) == 0)
						throw new CertificateException("Could not query the associated public key.");
					IntPtr buffer = Marshal.AllocHGlobal(length);
					try {
						SspiProvider.CertGetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_PROV_INFO_PROP_ID, buffer, ref length);
						CRYPT_KEY_PROV_INFO provInfo = (CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(buffer, typeof(CRYPT_KEY_PROV_INFO));
						CspParameters cspParams = new CspParameters();
						cspParams.KeyContainerName = provInfo.pwszContainerName;
						cspParams.ProviderName = provInfo.pwszProvName;
						cspParams.ProviderType = provInfo.dwProvType;
						cspParams.KeyNumber = provInfo.dwKeySpec;
						if((provInfo.dwFlags & 32) != 0)
							cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
						return cspParams;
					} finally {
						Marshal.FreeHGlobal(buffer);
					}
				}*/
		/// <summary>
		/// Converts an integer to a series of bytes.
		/// </summary>
		/// <param name="dwInput">The integer to convert.</param>
		/// <returns>An array of bytes that represents the integer.</returns>
		/// <remarks>This method returns the minimum required number of bytes to represent a specific integer number.</remarks>
		internal byte[] ConvertIntToByteArray(int dwInput) {
			int tempByte, size = 0;
			byte[] ret, temp = new byte[8];
			if (dwInput == 0)
				return new byte[1];
			tempByte = dwInput;
			// fill the temp array
			while (tempByte > 0) {
				temp[size] = (byte)(tempByte & 255);
				tempByte = tempByte >> 8;
				size++;
			}
			// copy the bytes from the temp buffer to the ret buffer in big endian order
			ret = new byte[size];
			if (BitConverter.IsLittleEndian) {
				for(int i = 0; i < size; i++)
					ret[i] = temp[size - i - 1];
			} else {
				for(int i = 0; i < size; i++)
					ret[i] = temp[i];
			}
			return ret;
		}
		/// <summary>
		/// Associates the certificate with a private key from a PVK file.
		/// </summary>
		/// <param name="pvkFile">The path to the PVK file to open.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <remarks>
		/// <p>The <paramref name="password"/> can be a null reference (<b>Nothing</b> in Visual Basic) if the private key is not encrypted.</p>
		/// <p>The private key will not be exportable.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The PVK file is encrypted and <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
		/// <exception cref="CertificateException">An error occurs while importing the private key.</exception>
		public void AssociateWithPrivateKey(string pvkFile, string password) {
			AssociateWithPrivateKey(pvkFile, password, false);
		}
		/// <summary>
		/// Associates the certificate with a private key from a PVK file.
		/// </summary>
		/// <param name="pvkFile">The path to the PVK file to open.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <param name="exportable"><b>true</b> if the private key should be marked exportable, <b>false</b> otherwise.</param>
		/// <remarks>The <paramref name="password"/> can be a null reference (<b>Nothing</b> in Visual Basic) if the private key is not encrypted.</remarks>
		/// <exception cref="ArgumentNullException">The PVK file is encrypted and <paramref name="password"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
		/// <exception cref="CertificateException">An error occurs while importing the private key.</exception>
		public void AssociateWithPrivateKey(string pvkFile, string password, bool exportable) {
			try {
				if (!File.Exists(pvkFile))
					throw new FileNotFoundException("The PVK file could not be found.");
			} catch (FileNotFoundException fnf) {
				throw fnf;
			} catch (Exception e) {
				throw new FileNotFoundException("The PVK file could not be found.", e);
			}
			byte[] buffer = new byte[24];
			FileStream fs = File.Open(pvkFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			fs.Read(buffer, 0, buffer.Length);
			if (BitConverter.ToUInt32(buffer, 0) != 0xB0B5F11E)
				throw new CertificateException("The specified file is not a valid PVK file.");
			int keytype = BitConverter.ToInt32(buffer, 8);
			int isEncrypted = BitConverter.ToInt32(buffer, 12);
			int saltLength = BitConverter.ToInt32(buffer, 16);
			int keyLen = BitConverter.ToInt32(buffer, 20);
			byte[] salt = new byte[saltLength];
			byte[] blob = new byte[keyLen];
			fs.Read(salt, 0, salt.Length);
			fs.Read(blob, 0, blob.Length);
			if (isEncrypted != 0) { // decrypt private key
				if (password == null)
					throw new ArgumentNullException();
				byte[] pass = Encoding.ASCII.GetBytes(password);
				byte[] key = new byte[salt.Length + password.Length];
				Array.Copy(salt, 0, key, 0, salt.Length);
				Array.Copy(pass, 0, key, salt.Length, pass.Length);
				byte[] pkb = TryDecrypt(blob, 8, blob.Length - 8, key, 16);
				if (pkb == null) { // decryption failed, try with an export key
					pkb = TryDecrypt(blob, 8, blob.Length - 8, key, 5);
					if (pkb == null) {
						throw new CertificateException("The PVK file could not be decrypted. [wrong password?]");
					}
				}
				Array.Copy(pkb, 0, blob, 8, pkb.Length);
				Array.Clear(pkb, 0, pkb.Length);
				Array.Clear(pass, 0, pass.Length);
				Array.Clear(key, 0, key.Length);
			}
			int hKey = 0, flags = 0;
			if (exportable)
				flags = SecurityConstants.CRYPT_EXPORTABLE;
			int provider = CAPIProvider.ContainerHandle;
			if (SspiProvider.CryptImportKey(provider, blob, blob.Length, 0, flags, ref hKey) == 0)
				throw new CertificateException("Could not import the private key from the PVK file.");
			CRYPT_KEY_PROV_INFO kpi = new CRYPT_KEY_PROV_INFO();
			kpi.pwszContainerName = SecurityConstants.KEY_CONTAINER;
			kpi.pwszProvName = null;
			kpi.dwProvType = SecurityConstants.PROV_RSA_FULL;
			kpi.dwFlags = 0;
			kpi.cProvParam = 0;
			kpi.rgProvParam = IntPtr.Zero;
			kpi.dwKeySpec = keytype;
			if (SspiProvider.CertSetCertificateContextProperty(this.Handle, SecurityConstants.CERT_KEY_PROV_INFO_PROP_ID, 0, ref kpi) == 0)
				throw new CertificateException("Could not associate the private key with the certificate.");
			SspiProvider.CryptDestroyKey(hKey);
			Array.Clear(blob, 0, blob.Length);
		}
		/// <summary>
		/// Exports the private key of this certificate to a PVK file.
		/// </summary>
		/// <param name="pvkFile">The path to the PVK file to create.</param>
		/// <param name="password">The password used to encrypt the private key.</param>
		/// <exception cref="CertificateException">An error occurs while exporting the private key.</exception>
		/// <exception cref="IOException">An error occurs while writing the PVK file.</exception>
		public void ExportPrivateKey(string pvkFile, string password) {
			if (!this.HasPrivateKey())
				throw new CertificateException("The certificate does not have an associated private key.");
			// export key
			int flags = 0, provider = 0, keySpec = 0, mustFree = 0, privateKey = 0, size = 0;
			if (!Environment.UserInteractive)
				flags = SecurityConstants.CRYPT_ACQUIRE_SILENT_FLAG;
			if (SspiProvider.CryptAcquireCertificatePrivateKey(this.Handle, flags, IntPtr.Zero, ref provider, ref keySpec, ref mustFree) == 0)
				throw new CertificateException("Could not acquire private key.");
			if (SspiProvider.CryptGetUserKey(provider, keySpec, ref privateKey) == 0)
				throw new CertificateException("Could not retrieve a handle of the private key.");
			if (SspiProvider.CryptExportKey(privateKey, 0, SecurityConstants.PRIVATEKEYBLOB, 0, IntPtr.Zero, ref size) == 0 && Marshal.GetLastWin32Error() != SecurityConstants.ERROR_MORE_DATA)
				throw new CertificateException("Could not export the private key.");
			byte[] buffer = new byte[size];
			if (SspiProvider.CryptExportKey(privateKey, 0, SecurityConstants.PRIVATEKEYBLOB, 0, buffer, ref size) == 0)
				throw new CertificateException("Could not export the private key.");
			if (mustFree != 0)
				SspiProvider.CryptReleaseContext(provider, 0);
			// initialize header fields and encrypt the data if necessary
			uint magic = 0xB0B5F11E;
			int reserved = 0, encrypted = (password == null ? 0 : 1), saltlen;
			byte[] salt;
			if (encrypted == 0) {
				salt = new byte[0];
			} else {
				salt = new byte[16];
				new RNGCryptoServiceProvider().GetBytes(salt);
				byte[] key = Encoding.ASCII.GetBytes(password);
				SHA1 sha1 = SHA1.Create();
				sha1.TransformBlock(salt, 0, salt.Length, salt, 0);
				sha1.TransformFinalBlock(key, 0, key.Length);
				key = new byte[16];
				Array.Copy(sha1.Hash, 0, key, 0, key.Length);
				ICryptoTransform rc4 = RC4.Create().CreateEncryptor(key, null);
				rc4.TransformBlock(buffer, 8, buffer.Length - 8, buffer, 8);
				rc4.Dispose();
				sha1.Clear();
			}
			saltlen = salt.Length;
			// write the PVK file
			FileStream fs = null;
			try {
				fs = File.Open(pvkFile, FileMode.Create, FileAccess.Write, FileShare.Read);
				fs.Write(BitConverter.GetBytes(magic), 0, 4);
				fs.Write(BitConverter.GetBytes(reserved), 0, 4);
				fs.Write(BitConverter.GetBytes(keySpec), 0, 4);
				fs.Write(BitConverter.GetBytes(encrypted), 0, 4);
				fs.Write(BitConverter.GetBytes(saltlen), 0, 4);
				fs.Write(BitConverter.GetBytes(size), 0, 4);
				if (salt.Length > 0)
					fs.Write(salt, 0, salt.Length);
				fs.Write(buffer, 0, buffer.Length);
			} catch (IOException ioe) {
				throw ioe;
			} catch (Exception e) {
				throw new IOException("An error occurs while writing the file.", e);
			} finally {
				if (fs != null)
					fs.Close();
			}
		}
		// return null is a decryption error occurs
		/// <summary>
		/// Tries decrypting the PRIVATEKEYBLOB blob.
		/// </summary>
		/// <param name="buffer">The buffer to decrypt.</param>
		/// <param name="offset">The starting offset.</param>
		/// <param name="length">The number of bytes to decrypt.</param>
		/// <param name="password">The password used to encrypt the PVK file (the salt should be prepended to the password).</param>
		/// <param name="keyLen">The effective key length in bytes (16 for 128 bit encryption, 5 for 40 bit encryption).</param>
		/// <returns>The decrypted buffer if successfull, or a null reference otherwise.</returns>
		private byte[] TryDecrypt(byte[] buffer, int offset, int length, byte[] password, int keyLen) {
			byte[] key = new byte[16];
			Array.Copy(SHA1.Create().ComputeHash(password, 0, password.Length), 0, key, 0, keyLen);
			byte[] ret = RC4.Create().CreateDecryptor(key, null).TransformFinalBlock(buffer, offset, length);
			if (ret[0] != 0x52 || ret[1] != 0x53 || ret[2] != 0x41 || ret[3] != 0x32) // first four bytes must be 'RSA2'
				return null;
			return ret;
		}
		/// <summary>
		/// Verifies whether this certificate has been revoked or not.
		/// </summary>
		/// <param name="crl">The encoded CRL to check against.</param>
		/// <exception cref="CertificateException">An error occurs while verifying the certificate.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="crl"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <returns><b>true</b> if the certificate is not on the CRL and therefore valid, or <b>false</b> if the certificate is on the CRL and therefore revoked.</returns>
		// Thanks go out to Gabriele Zannoni for implementing this method
		public bool VerifyRevocation(byte[] crl) {
			if (crl == null)
				throw new ArgumentNullException();
			IntPtr crlContextHandle = SspiProvider.CertCreateCRLContext(SecurityConstants.X509_ASN_ENCODING | SecurityConstants.PKCS_7_ASN_ENCODING, crl, crl.Length);
			if (crlContextHandle == IntPtr.Zero)
				throw new ArgumentException("The parameter is invalid.", "crl");
			IntPtr crle = IntPtr.Zero;
			if (SspiProvider.CertFindCertificateInCRL(Handle, crlContextHandle, 0, IntPtr.Zero, ref crle) == 0)
				throw new CertificateException("Unable to search the specified CRL for the certificate.");
			if (crle == IntPtr.Zero)
				return true;
			else
				return false;
		}
		/// <summary>
		/// The handle of the <see cref="Certificate"/> object.
		/// </summary>
		private IntPtr m_Handle;
		/// <summary>
		/// The handle of the <see cref="CertificateStore"/> object.
		/// </summary>
		private CertificateStore m_Store;
		/// <summary>
		/// A <see cref="CertificateInfo"/> instance associated with this certificate.
		/// </summary>
		internal CertificateInfo m_CertInfo;
		/// <summary>
		/// A <see cref="CertificateContext"/> instance associated with this certificate.
		/// </summary>
		internal CertificateContext m_Context;
		/// <summary>
		/// A reference to the associated <see cref="CertificateChain"/>.
		/// </summary>
		private CertificateChain m_Chain = null;
	}
}
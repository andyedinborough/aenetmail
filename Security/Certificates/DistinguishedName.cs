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
using System.Collections;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Represents a Relative Distinguished Name (RDN) of a <see cref="Certificate"/>.
	/// </summary>
	public class DistinguishedName {
		/// <summary>
		/// Initializes a new instance of the <see cref="DistinguishedName"/> class.
		/// </summary>
		public DistinguishedName() {
			m_List = new ArrayList();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DistinguishedName"/> class.
		/// </summary>
		/// <param name="cni">A <see cref="CertificateNameInfo"/> instance that's used to initialize the object.</param>
		internal DistinguishedName(CertificateNameInfo cni) : this() {
			Initialize(cni);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DistinguishedName"/> class.
		/// </summary>
		/// <param name="input">A pointer to a buffer that's used to initialize the object.</param>
		/// <param name="length">The length of the buffer.</param>
		/// <exception cref="CertificateException">Could not decode the buffer.</exception>
		internal DistinguishedName(IntPtr input, int length) : this() {
			int size = 0;
			SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, new IntPtr(SecurityConstants.X509_UNICODE_NAME), input, length, 0, IntPtr.Zero, ref size);
			if (size <= 0)
				throw new CertificateException("Unable to decode the name of the certificate.");
			IntPtr buffer = Marshal.AllocHGlobal(size);
			if (SspiProvider.CryptDecodeObject(SecurityConstants.PKCS_7_ASN_ENCODING | SecurityConstants.X509_ASN_ENCODING, new IntPtr(SecurityConstants.X509_UNICODE_NAME), input, length, 0, buffer, ref size) == 0)
				throw new CertificateException("Unable to decode the name of the certificate.");
			try {
				CertificateNameInfo cni = (CertificateNameInfo)Marshal.PtrToStructure(buffer, typeof(CertificateNameInfo));
				Initialize(cni);
			} catch (CertificateException ce) {
				throw ce;
			} catch (Exception e) {
				throw new CertificateException("Could not get the certificate distinguished name.", e);
			} finally {
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DistinguishedName"/> class with a given <see cref="CertificateNameInfo"/> instance.
		/// </summary>
		/// <param name="cni">The CertificateNameInfo instance to initialize from.</param>
		/// <exception cref="CertificateException">An error occurs while initializeing the DistinguishedName object.</exception>
		private void Initialize(CertificateNameInfo cni) {
			if (cni.cRDN <= 0)
				throw new CertificateException("Certificate does not have a subject relative distinguished name.");
			RelativeDistinguishedName cr;
			RdnAttribute cra;
			for(int i = 0; i < cni.cRDN; i++) {
				cr = (RelativeDistinguishedName)Marshal.PtrToStructure(new IntPtr(cni.rgRDN.ToInt64() + i * Marshal.SizeOf(typeof(RelativeDistinguishedName))), typeof(RelativeDistinguishedName));
				for(int j = 0; j < cr.cRDNAttr; j++) {
					cra = (RdnAttribute)Marshal.PtrToStructure(new IntPtr(cr.rgRDNAttr.ToInt64() + j * Marshal.SizeOf(typeof(RdnAttribute))), typeof(RdnAttribute));
					m_List.Add(new NameAttribute(Marshal.PtrToStringAnsi(cra.pszObjId), Marshal.PtrToStringUni(cra.pbData)));
				}
			}
		}
		/// <summary>
		/// Adds a <see cref="NameAttribute"/> to the end of the list.
		/// </summary>
		/// <param name="attribute">The NameAttribute to be added to the end of the list. </param>
		/// <returns>The index at which the value has been added.</returns>
		public int Add(NameAttribute attribute) {
			return m_List.Add(attribute);
		}
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set. </param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		public NameAttribute this[int index] {
			get {
				return (NameAttribute)m_List[index];
			}
			set {
				m_List[index] = value;
			}
		}
		/// <summary>
		/// Gets the number of elements actually contained in the list.
		/// </summary>
		/// <value>The number of elements actually contained in the list.</value>
		public int Count {
			get {
				return m_List.Count;
			}
		}
		/// <summary>
		/// Removes all elements from the list.
		/// </summary>
		public void Clear() {
			m_List.Clear();
		}
		/// <summary>
		/// Determines whether an element is in the list.
		/// </summary>
		/// <param name="value">The <see cref="NameAttribute"/> to locate in the list.</param>
		/// <returns><b>true</b> if item is found in the list; otherwise, <b>false</b>.</returns>
		public bool Contains(NameAttribute value) {
			return m_List.Contains(value);
		}
		/// <summary>
		/// Searches for the specified Object and returns the zero-based index of the first occurrence within the entire list.
		/// </summary>
		/// <param name="value">The <see cref="NameAttribute"/> to locate in the list.</param>
		/// <returns>The zero-based index of the first occurrence of value within the entire list, if found; otherwise, -1.</returns>
		public int IndexOf(NameAttribute value) {
			return m_List.IndexOf(value);
		}
		/// <summary>
		/// Searches for an Object with the specified Object identifier and returns the zero-based index of the first occurrence within the entire list.
		/// </summary>
		/// <param name="oid">The object identifier to search for.</param>
		/// <returns>The zero-based index of the first occurrence of value within the entire list, if found; otherwise, -1.</returns>
		public int IndexOf(string oid) {
			for(int i = 0; i < m_List.Count; i++) {
				if (((NameAttribute)m_List[i]).ObjectID == oid)
					return i;
			}
			return -1;
		}
		/// <summary>
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The <see cref="NameAttribute"/> to insert.</param>
		public void Insert(int index, NameAttribute value) {
			m_List.Insert(index, value);
		}
		/// <summary>
		/// Removes the first occurrence of a specific element from the list.
		/// </summary>
		/// <param name="value">The <see cref="NameAttribute"/> to remove from the list.</param>
		public void Remove(NameAttribute value) {
			m_List.Remove(value);
		}
		/// <summary>
		/// Removes the element at the specified index of the list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		public void RemoveAt(int index) {
			m_List.RemoveAt(index);
		}
		/// <summary>
		/// The internal list instance.
		/// </summary>
		private ArrayList m_List;
	}
}
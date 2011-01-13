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
	/// Defines a collection of certificate stores.
	/// </summary>
	public class CertificateStoreCollection : CertificateStore {
		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateStoreCollection"/> class.
		/// </summary>
		/// <param name="stores">An array of stores that should be added to the collection.</param>
		/// <exception cref="ArgumentNullException"><paramref name="stores"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">One of the <see cref="CertificateStore"/> objects in the array is a <see cref="CertificateStoreCollection"/> instance. This is not allowed to avoid circular dependencies.</exception>
		/// <exception cref="CertificateException">An error occurs while adding a certificate to the collection.</exception>
		public CertificateStoreCollection(CertificateStore[] stores) : base(SspiProvider.CertOpenStore(new IntPtr(SecurityConstants.CERT_STORE_PROV_COLLECTION), 0, 0, 0, null), false) {
			if (stores == null)
				throw new ArgumentNullException();
			for(int i = 0; i < stores.Length; i++) {
				if (stores[i].ToString() == this.ToString()) {
					// used in order to avoid circular dependencies
					throw new ArgumentException("A certificate store collection cannot hold other certificate store collections.");
				}
			}
			for(int i = 0; i < stores.Length; i++) {
				if (SspiProvider.CertAddStoreToCollection(this.Handle, stores[i].Handle, 0, 0) == 0)
					throw new CertificateException("Could not add the store to the collection.");
			}
			m_Stores = new ArrayList(); // used to hold references to the certificate stores so they cannot be finalized
			m_Stores.AddRange(stores);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateStoreCollection"/> class.
		/// </summary>
		/// <param name="collection">The CertificateStoreCollection whose elements are copied to the new certificate store collection.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while adding a certificate to the collection.</exception>
		public CertificateStoreCollection(CertificateStoreCollection collection) : base(SspiProvider.CertOpenStore(new IntPtr(SecurityConstants.CERT_STORE_PROV_COLLECTION), 0, 0, 0, null), false) {
			if (collection == null)
				throw new ArgumentNullException();
			m_Stores = new ArrayList( collection.m_Stores); // used to hold references to the certificate stores so they cannot be finalized
			for(int i = 0; i < m_Stores.Count; i++) {
				if (SspiProvider.CertAddStoreToCollection(this.Handle, ((CertificateStore)m_Stores[i]).Handle, 0, 0) == 0)
					throw new CertificateException("Could not add the store to the collection.");
			}
		}
		/// <summary>
		/// Adds a certificate store to the collection.
		/// </summary>
		/// <param name="store">An instance of the <see cref="CertificateStore"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The specified certificate store is a <see cref="CertificateStoreCollection"/> instance. This is not allowed to avoid circular dependencies.</exception>
		/// <exception cref="CertificateException">An error occurs while adding the certificate to the collection.</exception>
		public void AddStore(CertificateStore store) {
			if (store == null)
				throw new ArgumentNullException();
			if (store.ToString() == this.ToString()) // avoid circular dependencies
				throw new ArgumentException("A certificate store collection cannot hold other certificate store collections.");
			if (SspiProvider.CertAddStoreToCollection(this.Handle, store.Handle, 0, 0) == 0)
				throw new CertificateException("Could not add the store to the collection.");
			m_Stores.Add(store);
		}
		/// <summary>
		/// Removes a certificate store from the collection.
		/// </summary>
		/// <param name="store">An instance of the <see cref="CertificateStore"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="store"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		public void RemoveStore(CertificateStore store) {
			if (store == null)
				throw new ArgumentNullException();
			SspiProvider.CertRemoveStoreFromCollection(this.Handle, store.Handle);
			m_Stores.Remove(store);
		}
		/// <summary>
		/// Holds the references to the CertificateStore instances in the collection. This is to avoid CertificateStores finalizing and destroying their handles.
		/// </summary>
		private ArrayList m_Stores;
	}
}

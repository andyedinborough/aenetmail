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
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Defines a chain of certificates.
	/// </summary>
	public class CertificateChain {
		/// <summary>
		/// Initializes a new <see cref="CertificateChain"/> instance from a <see cref="Certificate"/>.
		/// </summary>
		/// <param name="cert">The certificate for which a chain is being built.</param>
		/// <remarks><paramref name="cert"/> will always be the end certificate.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="cert"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while building the certificate chain.</exception>
		public CertificateChain(Certificate cert) : this(cert, null) {}
		/// <summary>
		/// Initializes a new <see cref="CertificateChain"/> instance from a <see cref="Certificate"/>.
		/// </summary>
		/// <param name="cert">The certificate for which a chain is being built.</param>
		/// <param name="additional">Any additional store to be searched for supporting certificates and CTLs.</param>
		/// <remarks><paramref name="cert"/> will always be the end certificate.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="cert"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while building the certificate chain.</exception>
		public CertificateChain(Certificate cert, CertificateStore additional) : this(cert, additional, CertificateChainOptions.Default) {}
		/// <summary>
		/// Initializes a new <see cref="CertificateChain"/> instance from a <see cref="Certificate"/>.
		/// </summary>
		/// <param name="cert">The certificate for which a chain is being built.</param>
		/// <param name="additional">Any additional store to be searched for supporting certificates and CTLs.</param>
		/// <param name="options">Additional certificate chain options.</param>
		/// <remarks><paramref name="cert"/> will always be the end certificate.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="cert"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CertificateException">An error occurs while building the certificate chain.</exception>
		public CertificateChain(Certificate cert, CertificateStore additional, CertificateChainOptions options) {
			if (cert == null)
				throw new ArgumentNullException();
			IntPtr addstore = additional == null ? IntPtr.Zero : additional.Handle;
			ChainParameters para = new ChainParameters();
			para.cbSize = Marshal.SizeOf(typeof(ChainParameters));
			para.RequestedUsagecUsageIdentifier = 0;
			para.RequestedUsagedwType = 0;
			para.RequestedUsagergpszUsageIdentifier = IntPtr.Zero;
			if (SspiProvider.CertGetCertificateChain(IntPtr.Zero, cert.Handle, IntPtr.Zero, addstore, ref para, (int)options, IntPtr.Zero, ref m_Handle) == 0)
				throw new CertificateException("Unable to find the certificate chain.");
			m_Certificate = cert;
		}
		/// <summary>
		/// Disposes of the certificate chain.
		/// </summary>
		~CertificateChain() {
			if (m_Handle != IntPtr.Zero) {
				SspiProvider.CertFreeCertificateChain(m_Handle);
				m_Handle = IntPtr.Zero;
			}
		}
		/// <summary>
		/// Returns the certificate for which this chain was built.
		/// </summary>
		protected Certificate Certificate {
			get {
				return m_Certificate;
			}
		}
		/// <summary>
		/// Returns the list of certificates in this <see cref="CertificateChain"/>.
		/// </summary>
		/// <returns>An array of <see cref="Certificate"/> instances.</returns>
		/// <remarks>
		/// The certificate with index 0 is the end certificate in the chain, the certificate with the highest index is the root certificate [if it can be found].
		/// </remarks>
        // Thanks go out to Hernan de Lahitte and Neil for notifying us about a bug in this method.
		public virtual Certificate[] GetCertificates() {
			ArrayList ret = new ArrayList();
			int dwVerificationFlags;
			IntPtr store;
			CertificateStoreCollection csc, cs = this.Certificate.Store as CertificateStoreCollection;
			if (cs != null) {
				csc = new CertificateStoreCollection(cs);
			} else {
				csc = new CertificateStoreCollection(new CertificateStore[0]);
				if (this.Certificate.Store == null)
					csc.AddStore(new CertificateStore(this.Certificate.m_Context.hCertStore, true));
				else
					csc.AddStore(this.Certificate.Store);
			}
			csc.AddStore(CertificateStore.GetCachedStore(CertificateStore.RootStore));
			csc.AddStore(CertificateStore.GetCachedStore(CertificateStore.CAStore));
			store = csc.Handle;

			IntPtr cert = this.Certificate.DuplicateHandle();
			while(cert != IntPtr.Zero) {
				ret.Add(new Certificate(cert, false));
				dwVerificationFlags = 0;
				cert = SspiProvider.CertGetIssuerCertificateFromStore(store, cert, IntPtr.Zero, ref dwVerificationFlags);
			}

			csc.Dispose(); // don't use csc anymore after this line!!!

			return (Certificate[])ret.ToArray(typeof(Certificate));
		}
		/// <summary>
		/// Verifies the end <see cref="Certificate"/> according to the SSL policy rules.
		/// </summary>
		/// <param name="server">The server that returned the certificate -or- a null reference if the certificate is a client certificate.</param>
		/// <param name="type">One of the <see cref="AuthType"/> values.</param>
		/// <returns>One of the <see cref="CertificateStatus"/> values.</returns>
		/// <exception cref="CertificateException">An error occurs while verifying the certificate.</exception>
		public virtual CertificateStatus VerifyChain(string server, AuthType type) {
			return VerifyChain(server, type, VerificationFlags.None);
		}
		/// <summary>
		/// Verifies the end <see cref="Certificate"/> according to the SSL policy rules.
		/// </summary>
		/// <param name="server">The server that returned the certificate -or- a null reference if the certificate is a client certificate.</param>
		/// <param name="type">One of the <see cref="AuthType"/> values.</param>
		/// <param name="flags">One or more of the <see cref="VerificationFlags"/> values. VerificationFlags values can be combined with the OR operator.</param>
		/// <returns>One of the <see cref="CertificateStatus"/> values.</returns>
		/// <exception cref="CertificateException">An error occurs while verifying the certificate.</exception>
		public virtual CertificateStatus VerifyChain(string server, AuthType type, VerificationFlags flags) {
			// Convert the server string to a wide string memory pointer
			IntPtr serverName = IntPtr.Zero;
			IntPtr dataPtr = IntPtr.Zero;
			try {
				if (server == null) {
					serverName = IntPtr.Zero;
				} else {
					serverName = Marshal.StringToHGlobalUni(server);
				}
				// create a HTTPSPolicyCallbackData and get a memory pointer to the structure
				SslPolicyParameters data = new SslPolicyParameters();
				data.cbSize = Marshal.SizeOf(typeof(SslPolicyParameters));
				data.dwAuthType = (int)type;
				data.pwszServerName = serverName;
				data.fdwChecks = (int)flags;
				dataPtr = Marshal.AllocHGlobal(data.cbSize);
				Marshal.StructureToPtr(data, dataPtr, false);
				// create a CERT_CHAIN_POLICY_PARA
				ChainPolicyParameters para = new ChainPolicyParameters();
				para.cbSize = Marshal.SizeOf(typeof(ChainPolicyParameters));
				para.dwFlags = (int)flags;
				para.pvExtraPolicyPara = dataPtr;
				// create a CERT_CHAIN_POLICY_STATUS
				ChainPolicyStatus status = new ChainPolicyStatus();
				status.cbSize = Marshal.SizeOf(typeof(ChainPolicyStatus)); 
				// verify the certificate
				if (SspiProvider.CertVerifyCertificateChainPolicy(new IntPtr(SecurityConstants.CERT_CHAIN_POLICY_SSL), m_Handle, ref para, ref status) == 0)
					throw new CertificateException("Unable to verify the certificate.");
				if (Enum.IsDefined(typeof(CertificateStatus), status.dwError))
					return (CertificateStatus)status.dwError;
				else
					return CertificateStatus.OtherError;
			} finally {
				// clean up
				if (dataPtr != IntPtr.Zero)
					Marshal.FreeHGlobal(dataPtr);
				if (serverName != IntPtr.Zero)
					Marshal.FreeHGlobal(serverName);
			}
		}
		/// <summary>
		/// Verifies the end <see cref="Certificate"/> according to the SSL policy rules.
		/// </summary>
		/// <param name="server">The server that returned the certificate -or- a null reference if the certificate is a client certificate.</param>
		/// <param name="type">One of the <see cref="AuthType"/> values.</param>
		/// <param name="flags">One or more of the <see cref="VerificationFlags"/> values. VerificationFlags values can be combined with the OR operator.</param>
		/// <param name="crl">An optional CRL to check. This parameter can be null (<b>Nothing</b> in Visual Basic).</param>
		/// <returns>One of the <see cref="CertificateStatus"/> values.</returns>
		/// <exception cref="CertificateException">An error occurs while verifying the certificate.</exception>
		/// <remarks>Only the leaf certificate is checked against the CRL.</remarks>
		// Thanks go out to Gabriele Zannoni for implementing this method
		public virtual CertificateStatus VerifyChain(string server, AuthType type, VerificationFlags flags, byte[] crl) {
			CertificateStatus status = VerifyChain(server, type, flags);
			if (status != CertificateStatus.ValidCertificate || crl == null)
				return status;
			try {
				if (!m_Certificate.VerifyRevocation(crl))
					return CertificateStatus.Revoked;
				else
					return status;
			} catch {
				return CertificateStatus.RevocationFailure;
			}
		}
		/// <summary>
		/// Begins verification of the end <see cref="Certificate"/> according to the SSL policy rules.
		/// </summary>
		/// <param name="server">The server that returned the certificate -or- a null reference if the certificate is a client certificate.</param>
		/// <param name="type">One of the <see cref="AuthType"/> values.</param>
		/// <param name="flags">One or more of the <see cref="VerificationFlags"/> values. VerificationFlags values can be combined with the OR operator.</param>
		/// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="asyncState">An object that contains state information for this request.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous connection.</returns>
		/// <exception cref="CertificateException">An error occurs while queuing the verification request.</exception>
		public virtual IAsyncResult BeginVerifyChain(string server, AuthType type, VerificationFlags flags, AsyncCallback callback, object asyncState) {
			CertificateVerificationResult ret = new CertificateVerificationResult(this, server, type, flags, callback, asyncState);
			if (!ThreadPool.QueueUserWorkItem(new WaitCallback(this.StartVerification), ret))
				throw new CertificateException("Could not schedule the certificate chain for verification.");
			return ret;
		}
		/// <summary>
		/// Ends a pending asynchronous certificate verification request.
		/// </summary>
		/// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
		/// <returns>One of the <see cref="CertificateStatus"/> values.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="ar"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException">The <paramref name="ar"/> parameter was not returned by a call to the <see cref="BeginVerifyChain"/> method.</exception>
		/// <exception cref="InvalidOperationException"><b>EndVerifyChain</b> was previously called for the asynchronous chain verification.</exception>
		/// <exception cref="CertificateException">An error occurs while verifying the certificate chain.</exception>
		public virtual CertificateStatus EndVerifyChain(IAsyncResult ar) {
			if (ar == null)
				throw new ArgumentNullException();
			CertificateVerificationResult result;
			try {
				result = (CertificateVerificationResult)ar;
			} catch {
				throw new ArgumentException();
			}
			if (result.Chain != this)
				throw new ArgumentException();
			if (result.HasEnded)
				throw new InvalidOperationException();
			if (result.ThrowException != null)
				throw result.ThrowException;
			result.HasEnded = true;
			return result.Status;
		}
		/// <summary>
		/// Verifies a certificate chain and calls a delegate when finished.
		/// </summary>
		/// <param name="state">Stores state information for this asynchronous operation as well as any user-defined data.</param>
		protected void StartVerification(object state) {
			if (state == null)
				return;
			CertificateVerificationResult result;
			try {
				result = (CertificateVerificationResult)state;
			} catch {
				return;
			}
			CertificateStatus ret;
			try {
				ret = VerifyChain(result.Server, result.Type, result.Flags);
			} catch (CertificateException ce)  {
				result.VerificationCompleted(ce, CertificateStatus.OtherError);
				return;
			} catch (Exception e) {
				result.VerificationCompleted(new CertificateException("Could not verify the certificate chain.", e), CertificateStatus.OtherError);
				return;
			}
			result.VerificationCompleted(null, ret);
		}
		/// <summary>
		/// The handle of the certificate chain.
		/// </summary>
		private IntPtr m_Handle;
		/// <summary>
		/// The end certificate that was used to build the chain.
		/// </summary>
		private Certificate m_Certificate;
	}
}

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
using Org.Mentalis.Security;

namespace Org.Mentalis.Security.Certificates {
	/// <summary>
	/// Represents the status of an asynchronous certificate chain verification operation.
	/// </summary>
	internal class CertificateVerificationResult : IAsyncResult {
		/// <summary>
		/// Initializes a new CertificateVerificationResult instance.
		/// </summary>
		/// <param name="chain">The <see cref="CertificateChain"/> that has to be verified.</param>
		/// <param name="server">The server to which the <see cref="Certificate"/> has been issued.</param>
		/// <param name="type">One of the <see cref="AuthType"/> values.</param>
		/// <param name="flags">One of the <see cref="VerificationFlags"/> values.</param>
		/// <param name="callback">The delegate to call when the verification finishes.</param>
		/// <param name="asyncState">User-defined state data.</param>
		public CertificateVerificationResult(CertificateChain chain, string server, AuthType type, VerificationFlags flags, AsyncCallback callback, object asyncState) {
			m_Chain = chain;
			m_Server = server;
			m_Type = type;
			m_Flags = flags;
			m_AsyncState = asyncState;
			m_Callback = callback;
			m_WaitHandle = null;
			m_HasEnded = false;
		}
		/// <summary>
		/// Gets an indication of whether the asynchronous operation completed synchronously.
		/// </summary>
		/// <value>Always <b>false</b>.</value>
		public bool CompletedSynchronously {
			get {
				return false;
			}
		}
		/// <summary>
		/// Gets a boolean value that indicates whether the operation has finished.
		/// </summary>
		/// <value>
		/// <b>true</b> if the verification of the chain has been completed, <b>false</b> otherwise.
		/// </value>
		public bool IsCompleted {
			get {
				return m_IsCompleted;
			}
		}
		/// <summary>
		/// Gets a <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.
		/// </summary>
		/// <value>
		/// A WaitHandle that is used to wait for an asynchronous operation to complete.
		/// </value>
		public WaitHandle AsyncWaitHandle {
			get {
				if (m_WaitHandle == null)
					m_WaitHandle = new ManualResetEvent(false);
				return m_WaitHandle;
			}
		}
		/// <summary>
		/// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
		/// </summary>
		/// <value>
		/// A user-defined object that qualifies or contains information about an asynchronous operation.
		/// </value>
		public object AsyncState {
			get {
				return m_AsyncState;
			}
		}
		/// <summary>
		/// Sets the WaitHandle to signalled and calls the appropriate delegate.
		/// </summary>
		/// <param name="error">An exception that may have occurred.</param>
		/// <param name="status">The status of the certificate chain.</param>
		internal void VerificationCompleted(Exception error, CertificateStatus status) {
			m_ThrowException = error;
			m_Status = status;
			m_IsCompleted = true;
			if (m_Callback != null)
				m_Callback(this);
			if (m_WaitHandle != null)
				m_WaitHandle.Set();
		}
		/// <summary>
		/// Gets the associated certificate chain.
		/// </summary>
		/// <value>
		/// A <see cref="CertificateChain"/> instance.
		/// </value>
		public CertificateChain Chain {
			get {
				return m_Chain;
			}
		}
		/// <summary>
		/// Gets the associated server name.
		/// </summary>
		/// <value>
		/// A string that holds the server name.
		/// </value>
		public string Server {
			get {
				return m_Server;
			}
		}
		/// <summary>
		/// Gets the associated authentication type.
		/// </summary>
		/// <value>
		/// One of the <see cref="AuthType"/> values.
		/// </value>
		public AuthType Type {
			get {
				return m_Type;
			}
		}
		/// <summary>
		/// Gets the associated verification flags.
		/// </summary>
		/// <value>
		/// One of the <see cref="VerificationFlags"/> values.
		/// </value>
		public VerificationFlags Flags {
			get {
				return m_Flags;
			}
		}
		/// <summary>
		/// Gets or sets a value that indicates whether the user has called EndVerifyChain for this object.
		/// </summary>
		/// <value>
		/// <b>true</b> if the user has called EndVerifyChain, <b>false</b> otherwise.
		/// </value>
		public bool HasEnded {
			get {
				return m_HasEnded;
			}
			set {
				m_HasEnded = value;
			}
		}
		/// <summary>
		/// Gets an exception that has occurred while verifying the certificate chain or a null reference (<b>Nothing</b> in Visual Basic) if the verification succeeded.
		/// </summary>
		/// <value>
		/// A <see cref="Exception"/> instance.
		/// </value>
		public Exception ThrowException {
			get {
				return m_ThrowException;
			}
		}
		/// <summary>
		/// Gets the status of the <see cref="CertificateChain"/>.
		/// </summary>
		/// <value>
		/// One of the <see cref="CertificateStatus"/> values.
		/// </value>
		public CertificateStatus Status {
			get {
				return m_Status;
			}
		}
		/// <summary>Holds the value of the IsCompleted property.</summary>
		private bool m_IsCompleted;
		/// <summary>Holds the value of the AsyncState property.</summary>
		private object m_AsyncState;
		/// <summary>Holds the value of the Chain property.</summary>
		private CertificateChain m_Chain;
		/// <summary>Holds the value of the Server property.</summary>
		private string m_Server;
		/// <summary>Holds the value of the Type property.</summary>
		private AuthType m_Type;
		/// <summary>Holds the value of the Flags property.</summary>
		private VerificationFlags m_Flags;
		/// <summary>Holds the value of the WaitHandle property.</summary>
		private ManualResetEvent m_WaitHandle;
		/// <summary>Holds the value of the Callback property.</summary>
		private AsyncCallback m_Callback;
		/// <summary>Holds the value of the HasEnded property.</summary>
		private bool m_HasEnded;
		/// <summary>Holds the value of the ThrowException property.</summary>
		private Exception m_ThrowException;
		/// <summary>Holds the value of the Status property.</summary>
		private CertificateStatus m_Status;
	}
}
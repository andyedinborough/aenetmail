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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.InteropServices;
using Org.Mentalis.Security.Certificates;

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Provides data for the <see cref="CertRequestEventHandler"/> event.
	/// </summary>
	/// <remarks>
	/// This class is used when a CertRequestEventHandler delegate is called. Application code can set a <see cref="Certificate"/> instance that will then be subseuently used by the <see cref="SecureSocket"/> to send to the peer.
	/// </remarks>
	public class RequestEventArgs {
		/// <summary>
		/// Initializes a new <see cref="RequestEventArgs"/> instance.
		/// </summary>
		public RequestEventArgs() : this(null) {}
		/// <summary>
		/// Initializes a new <see cref="RequestEventArgs"/> instance.
		/// </summary>
		/// <param name="cert">A <see cref="Certificate"/> instance.</param>
		public RequestEventArgs(Certificate cert) {
			m_Certificate = cert;
		}
		/// <summary>
		/// Gets or sets the <see cref="Certificate"/> that should be sent to the remote host.
		/// </summary>
		/// <value>A Certificate instance.</value>
		public Certificate Certificate {
			get {
				return m_Certificate;
			}
			set {
				m_Certificate = value;
			}
		}
		/// <summary>
		/// Holds the Certificate instance.
		/// </summary>
		private Certificate m_Certificate;
	}
}

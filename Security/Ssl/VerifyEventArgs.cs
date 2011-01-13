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
	/// Provides data for the Verify event.
	/// </summary>
	/// <remarks>
	/// When the CertVerifyEventHandler delegate is called, one of its parameters is an instance of this class. If the certificate should not be accepted and the connection should be closed, simply set the Valid property to <b>false</b> and return from the callback.
	/// </remarks>
	public class VerifyEventArgs {
		/// <summary>
		/// Initializes a new VerifyEventArgs instance.
		/// </summary>
		/// <remarks>The initial value of the <see cref="Valid"/> property will be <b>true</b>.</remarks>
		public VerifyEventArgs() : this(true) {}
		/// <summary>
		/// Initializes a new VerifyEventArgs instance.
		/// </summary>
		/// <param name="valid">The initial value of the <see cref="Valid"/> property.</param>
		public VerifyEventArgs(bool valid) {
			m_Valid = valid;
		}
		/// <summary>
		/// Gets or sets whether the certificate should be accepted as a valid certficate or not.
		/// </summary>
		/// <value><b>true</b> if the certificate is valid, otherwise <b>false</b>.</value>
		public bool Valid {
			get {
				return m_Valid;
			}
			set {
				m_Valid = value;
			}
		}
		/// <summary>
		/// Holds the value of the <see cref="Valid"/> property.
		/// </summary>
		private bool m_Valid;
	}
}

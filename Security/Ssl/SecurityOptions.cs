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
	/// Represents the security options that should be used when connecting to a secure server, or when accepting secure connections.
	/// </summary>
	public class SecurityOptions : ICloneable {
		/// <summary>
		/// Initializes a new instance of the SecurityOptions class.
		/// </summary>
		/// <param name="protocol">One of the <see cref="SecureProtocol"/> values.</param>
		/// <param name="cert">A <see cref="Certificate"/> instance.</param>
		/// <param name="entity">One of the <see cref="ConnectionEnd"/> values.</param>
		/// <param name="verifyType">One of the <see cref="CredentialVerification"/> values.</param>
		/// <param name="verifier">The <see cref="CertVerifyEventHandler"/> delegate.</param>
		/// <param name="commonName">The common name of the remote computer. This is usually a domain name.</param>
		/// <param name="flags">A bitwise combination of the <see cref="SecurityFlags"/> values.</param>
		/// <param name="allowed">A bitwise combination of the <see cref="SslAlgorithms"/> values.</param>
		/// <param name="requestHandler">The <see cref="CertRequestEventHandler"/> delegate.</param>
		public SecurityOptions(SecureProtocol protocol, Certificate cert, ConnectionEnd entity, CredentialVerification verifyType, CertVerifyEventHandler verifier, string commonName, SecurityFlags flags, SslAlgorithms allowed, CertRequestEventHandler requestHandler) {
			this.Protocol = protocol;
			this.Certificate = cert;
			this.Entity = entity;
			this.VerificationType = verifyType;
			this.Verifier = verifier;
			this.CommonName = commonName;
			this.Flags = flags;
			this.AllowedAlgorithms = allowed;
			this.RequestHandler = requestHandler;
		}
		/// <summary>
		/// Initializes a new instance of the SecurityOptions class.
		/// </summary>
		/// <param name="protocol">One of the <see cref="SecureProtocol"/> values.</param>
		/// <param name="cert">A <see cref="Certificate"/> instance.</param>
		/// <param name="entity">One of the <see cref="ConnectionEnd"/> values.</param>
		/// <remarks>
		/// All other members of the structure will be instantiated with default values.
		/// </remarks>
		public SecurityOptions(SecureProtocol protocol, Certificate cert, ConnectionEnd entity) : this(protocol, cert, entity, CredentialVerification.Auto, null, null, SecurityFlags.Default, SslAlgorithms.ALL, null) {}
		/// <summary>
		/// Initializes a new instance of the SecurityOptions structure.
		/// </summary>
		/// <param name="protocol">One of the <see cref="SecureProtocol"/> values.</param>
		/// <remarks>
		/// All other members of the structure will be instantiated with default values.
		/// </remarks>
		public SecurityOptions(SecureProtocol protocol) : this(protocol, null, ConnectionEnd.Client, CredentialVerification.Auto, null, null, SecurityFlags.Default, SslAlgorithms.ALL, null) {}
		/// <summary>
		/// Gets or sets the secure protocol that the <see cref="SecureSocket"/> should use.
		/// </summary>
		/// <value>A bitwise combination of the <see cref="SecureProtocol"/> values.</value>
		public SecureProtocol Protocol {
			get {
				return m_Protocol;
			}
			set {
				m_Protocol = value;
			}
		}
		/// <summary>
		/// Gets or sets the <see cref="Certificate"/> that the <see cref="SecureSocket"/> should use.
		/// </summary>
		/// <value>An instance of the Certificate class.</value>
		public Certificate Certificate {
			get {
				return m_Certificate;
			}
			set {
				m_Certificate = value;
			}
		}
		/// <summary>
		/// Gets or sets a value that indicates whether the <see cref="SecureSocket"/> is a server or a client socket.
		/// </summary>
		/// <value>One of the <see cref="ConnectionEnd"/> values.</value>
		public ConnectionEnd Entity {
			get {
				return m_Entity;
			}
			set {
				m_Entity = value;
			}
		}
		/// <summary>
		/// Gets or sets a value that indicates how the <see cref="SecureSocket"/> will try to verify the peer <see cref="Certificate"/>.
		/// </summary>
		/// <value>One of the <see cref="CredentialVerification"/> values.</value>
		public CredentialVerification VerificationType {
			get {
				return m_VerificationType;
			}
			set {
				m_VerificationType = value;
			}
		}
		/// <summary>
		/// Gets or sets a delegate that will be called when the <see cref="SecureSocket"/> receives the peer certificate.
		/// </summary>
		/// <value>A <see cref="CertVerifyEventHandler"/> delegate.</value>
		/// <remarks>This member will only be used if the <see cref="VerificationType"/> is set to Manual.</remarks>
		public CertVerifyEventHandler Verifier {
			get {
				return m_Verifier;
			}
			set {
				m_Verifier = value;
			}
		}
		/// <summary>
		/// Gets or sets a delegate that will be called when the <see cref="SecureSocket"/> receives a request for a client certificate.
		/// </summary>
		/// <value>A <see cref="CertRequestEventHandler"/> delegate.</value>
		/// <remarks>This member will only be used if no <see cref="Certificate"/> is specified in the Certificate property of this class.</remarks>
		public CertRequestEventHandler RequestHandler {
			get {
				return m_RequestHandler;
			}
			set {
				m_RequestHandler = value;
			}
		}
		/// <summary>
		/// Gets or sets the common name of the peer.
		/// </summary>
		/// <value>A <see cref="String"/> that holds the common name of the peer. This is usually a domain name.</value>
		/// <remarks>Servers that do not use client authentication should set this member to a null reference (<b>Nothing</b> in Visual Basic).</remarks>
		public string CommonName {
			get {
				return m_CommonName;
			}
			set {
				m_CommonName = value;
			}
		}
		/// <summary>
		/// Gets or sets the security flags associated with the <see cref="SecureSocket"/>.
		/// </summary>
		/// <value>A bitwise combination of the <see cref="SecurityFlags"/> values.</value>
		public SecurityFlags Flags {
			get {
				return m_Flags;
			}
			set {
				m_Flags = value;
			}
		}
		/// <summary>
		/// Gets or sets the list of algorithms that can be used to encrypt and compress data.
		/// </summary>
		/// <value>A bitwise combination of the <see cref="SslAlgorithms"/> values.</value>
		/// <remarks>
		/// This member should always contain at least one encryption algorithm and one compression algorithm.
		/// Currently, the only defined compression algorithm is SslAlgorithms.NULL_COMPRESSION.
		/// The default setting for this member is SslAlgorithms.ALL.
		/// </remarks>
		public SslAlgorithms AllowedAlgorithms {
			get {
				return m_AllowedAlgorithms;
			}
			set {
				m_AllowedAlgorithms = value;
			}
		}
		/// <summary>
		/// Creates a shallow copy of this <see cref="SecurityOptions"/> object.
		/// </summary>
		/// <returns>A shallow copy of this object.</returns>
		public object Clone() {
			return new SecurityOptions(this.Protocol, this.Certificate, this.Entity, this.VerificationType, this.Verifier, this.CommonName, this.Flags, this.AllowedAlgorithms, this.RequestHandler);
		}
		/// <summary>One of the <see cref="SecureProtocol"/> values.</summary>
		private SecureProtocol m_Protocol;
		/// <summary>A <see cref="Certificate"/> instance.</summary>
		private Certificate m_Certificate;
		/// <summary>One of the <see cref="ConnectionEnd"/> values.</summary>
		private ConnectionEnd m_Entity;
		/// <summary>One of the <see cref="CredentialVerification"/> values.</summary>
		private CredentialVerification m_VerificationType;
		/// <summary>The <see cref="CertVerifyEventHandler"/> delegate.</summary>
		private CertVerifyEventHandler m_Verifier;
		/// <summary>The <see cref="CertRequestEventHandler"/> delegate.</summary>
		private CertRequestEventHandler m_RequestHandler;
		/// <summary>The common name of the remote computer. This is usually a domain name.</summary>
		private string m_CommonName;
		/// <summary>A bitwise combination of the <see cref="SecurityFlags"/> values.</summary>
		private SecurityFlags m_Flags;
		/// <summary>A bitwise combination of the <see cref="SslAlgorithms"/> values.</summary>
		private SslAlgorithms m_AllowedAlgorithms;
	}
}
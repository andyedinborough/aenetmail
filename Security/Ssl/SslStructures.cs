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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Org.Mentalis.Security.Certificates;

namespace Org.Mentalis.Security.Ssl {
	/// <summary>
	/// Specifies the type of security protocol that an instance of the <see cref="SecureSocket"/> class can use.
	/// </summary>
	[Flags]
	public enum SecureProtocol : int {
		/// <summary>No security protocol will be used. The SecureSocket will act as a normal Socket.</summary>
		None = 0,
		/// <summary>SSLv3 will be used to authenticate the client and encrypt the data.</summary>
		Ssl3 = 2,
		/// <summary>TLS will be used to authenticate the client and encrypt the data.</summary>
		Tls1 = 4
	}
	/// <summary>
	/// Specifies the different security flags that an instance of the <see cref="SecureSocket"/> class can use.
	/// </summary>
	[Flags]
	public enum SecurityFlags : int {
		/// <summary>No special behavior is required.</summary>
		Default = 0x0,
		/// <summary>
		/// Client authentication is required. This flag only has an effect on server sockets.
		/// </summary>
		MutualAuthentication = 0x1,
		/// <summary>
		/// To avoid a certain CBC IV attack, the Security Library sends an empty message after the handshake and before the actual application payload.
		/// Unfortunately, some broken implementations do not support empty packets, so sending these empty packets can be turned off
		/// by specifying the DontSendEmptyRecord flag.
		/// </summary>
		DontSendEmptyRecord = 0x2,
		/// <summary>
		/// Setting this flag will allow a client to issue a SSLv3.0 version number as latest version supported in the premaster secret, even when TLSv1.0 (version 3.1) was announced in the client hello. Normally this is forbidden to prevent version rollback attacks.
		/// </summary>
		IgnoreMaxProtocol = 0x4
	}
	/// <summary>
	/// Specifies the different connection end values.
	/// </summary>
	public enum ConnectionEnd {
		/// <summary>The <see cref="SecureSocket"/> is a server socket.</summary>
		Server,
		/// <summary>The <see cref="SecureSocket"/> is a client socket.</summary>
		Client
	}
	/// <summary>
	/// Specifies the different cipher suites and compression algorithms.
	/// </summary>
	[Flags]
	public enum SslAlgorithms : int {
		/// <summary>No encryption or compression.</summary>
		NONE = 0x0,
		/// <summary>RC4 encryption with a 40 bit key and an MD5 hash.</summary>
		RSA_RC4_40_MD5 = 0x1,
		/// <summary>RC4 encryption with a 128 bit key and an MD5 hash.</summary>
		RSA_RC4_128_MD5 = 0x2,
		/// <summary>RC4 encryption with a 128 bit key and a SHA1 hash.</summary>
		RSA_RC4_128_SHA = 0x4,
		/// <summary>RC2 encryption with a 40 bit key and an MD5 hash.</summary>
		RSA_RC2_40_MD5 = 0x8,
		/// <summary>DES encryption with a 56 bit key and a SHA1 hash.</summary>
		RSA_DES_56_SHA = 0x10,
		/// <summary>Triple DES encryption with a 168 bit key and a SHA1 hash.</summary>
		RSA_3DES_168_SHA = 0x20,
		/// <summary>DES encryption with a 40 bit key and a SHA1 hash.</summary>
		RSA_DES_40_SHA = 0x40,
		/// <summary>AES encryption with a 128 bit key and a SHA1 hash.</summary>
		RSA_AES_128_SHA = 0x80,
		/// <summary>AES encryption with a 256 bit key and a SHA1 hash.</summary>
		RSA_AES_256_SHA = 0x100,
		/// <summary>Cipher Suites that are currently considered secure. As a convenience, this value also specifies NULL compression.</summary>
		SECURE_CIPHERS = RSA_AES_256_SHA | RSA_AES_128_SHA | RSA_RC4_128_SHA | RSA_RC4_128_MD5 | RSA_3DES_168_SHA | NULL_COMPRESSION,
		/// <summary>No compression. This value must always be specified; it is currently the only supported compression algorithm.</summary>
		NULL_COMPRESSION = 0x100000,
		/// <summary>All encryption and compression algorithms.</summary>
		ALL = 0x7FFFFFFF // 31x bit '1'
	}
	/// <summary>
	/// Specifies the method used to verify the remote credential.
	/// </summary>
	public enum CredentialVerification : int {
		/// <summary>The remote certificate will be manually verified. When an incoming connection is accepted, the SecureSocket will raise a CertVerification event. This is the recommended credential verification method.</summary>
		Manual,
		/// <summary>The remote certificate will be automatically verified by the crypto API.</summary>
		Auto,
		/// <summary>The remote certificate will be automatically verified by the crypto API, but the common name of the server will not be checked.</summary>
		AutoWithoutCName,
		/// <summary>The remote certificate will not be verified. This method is not secure and should only be used for debugging purposes.</summary>
		None
	}
	/// <summary>
	/// References the method to be called when the remote certificate should be verified.
	/// </summary>
	/// <param name="socket">The <see cref="SecureSocket"/> that received the certificate to verify.</param>
	/// <param name="remote">The <see cref="Certificate"/> of the remote party to verify. This parameter is a null reference (<b>Nothing</b> in Visual Basic) if the other side sent an empty certificate message.</param>
	/// <param name="chain">The <see cref="CertificateChain"/> associated with the remote certificate. This parameter is a null reference (<b>Nothing</b> in Visual Basic) if the other side sent an empty certificate message.</param>
	/// <param name="e">A <see cref="VerifyEventArgs"/> instance used to (in)validate the certificate. If this parameter is <b>true</b> after the delegate returns, the SecureSocket will continue the connection. If this parameter is <b>false</b> after the delegate returns, the connection will be closed.</param>
	/// <remarks>
	/// If an error is thrown by the code in the delegate, the SecureSocket will close the connection.
	/// </remarks>
	public delegate void CertVerifyEventHandler(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e);
	/// <summary>
	/// References the method to be called when the <see cref="SecureSocket"/> receives a <see cref="Certificate"/> request from the peer.
	/// </summary>
	/// <param name="socket">The SecureSocket that received the certificate request.</param>
	/// <param name="acceptable">An instance of the <see cref="DistinguishedNameList"/> class that contains a list of relative distinguished names. If the client chooses to send a certificate to the remote server, the CA that signed this certificate should be in the list of distinguished names.</param>
	/// <param name="e">A <see cref="RequestEventArgs"/> instance used to pass the certificate to the SecureSocket.</param>
	/// <remarks>
	/// <p>This delegate is only used by client sockets</p>
	/// <p>If an error is thrown by the code in the delegate, the SecureSocket will close the connection.</p>
	/// </remarks>
	public delegate void CertRequestEventHandler(SecureSocket socket, DistinguishedNameList acceptable, RequestEventArgs e);

	internal enum ControlType {
		Shutdown,
		Renegotiate,
		ClientHello
	}
	internal enum DataType {
		ApplicationData,
		ProtocolData
	}
	internal enum SslStatus {
		OK,
		ContinueNeeded,
		MessageIncomplete,
		Close
	}
	internal enum HashType {
		MD5,
		SHA1
	}
}
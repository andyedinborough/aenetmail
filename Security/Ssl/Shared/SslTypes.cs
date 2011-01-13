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
using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Ssl.Shared {
	internal enum ContentType : byte {
		ChangeCipherSpec = 20,
		Alert = 21,
		Handshake = 22,
		ApplicationData = 23
	}
	internal enum MessageType {
		PlainText,
		Encrypted
	}
	internal enum AlertLevel : byte {
		Warning = 1,
		Fatal = 2
	}
	internal struct ProtocolVersion {
		public ProtocolVersion(byte major, byte minor) {
			this.major = major;
			this.minor = minor;
		}
		public int GetVersionInt() {
			return major * 10 + minor;
		}
		public override string ToString() {
			return major.ToString() + "." + minor.ToString();
		}
		public byte major;
		public byte minor;
	}
	internal struct SslRecordStatus {
		public SslRecordStatus(SslStatus status, byte[] buffer, byte[] decrypted) {
			this.Status = status;
			this.Buffer = buffer;
			this.Decrypted = decrypted;
		}
		public SslStatus Status;
		public byte[] Buffer;
		public byte[] Decrypted;
	}
	internal struct SslHandshakeStatus {
		public SslHandshakeStatus(SslStatus status, byte[] message) {
			this.Status = status;
			this.Message = message;
		}
		public SslStatus Status;
		public byte[] Message;
	}
	internal struct CompatibilityResult {
		public CompatibilityResult(RecordLayer rl, SslRecordStatus status) {
			this.RecordLayer = rl;
			this.Status = status;
		}
		public RecordLayer RecordLayer;
		public SslRecordStatus Status;
	}
	internal enum HandshakeType : byte {
		HelloRequest = 0,
		ClientHello = 1,
		ServerHello = 2,
		Certificate = 11,
		ServerKeyExchange = 12,
		CertificateRequest = 13,
		ServerHelloDone = 14,
		CertificateVerify = 15,
		ClientKeyExchange = 16,
		Finished = 20,
		ShuttingDown = 253,			// not part of TLS standard; used internally in the Security Library
		ChangeCipherSpec = 254,		// not part of TLS standard; used internally in the Security Library
		Nothing = 255				// not part of TLS standard; used internally in the Security Library
	}
	internal enum AlertDescription : byte {
		CloseNotify = 0,
		UnexpectedMessage = 10,			// FATAL
		BadRecordMac = 20,				// FATAL
		DecryptionFailed = 21,			// FATAL
		RecordOverflow = 22,			// FATAL
		DecompressionFailure = 30,		// FATAL
		HandshakeFailure = 40,			// FATAL
		BadCertificate = 42,			// WARNING
		UnsupportedCertificate = 43,	// WARNING
		CertificateRevoked = 44,		// WARNING
		CertificateExpired = 45,		// WARNING
		CertificateUnknown = 46,		// WARNING
		IllegalParameter = 47,			// FATAL
		UnknownCa = 48,					// FATAL
		AccessDenied = 49,				// FATAL
		DecodeError = 50,				// FATAL
		DecryptError = 51,				// FATAL
		ExportRestriction = 60,			// FATAL
		ProtocolVersion = 70,			// FATAL
		InsufficientSecurity = 71,		// FATAL
		InternalError = 80,				// FATAL
		UserCanceled = 90,				// WARNING
		NoRenegotiation = 100			// WARNING
	}
	internal enum HashUpdate {
		Local,
		Remote,
		LocalRemote,
		All
	}
}
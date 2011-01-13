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
using System.Security.Cryptography;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Ssl.Ssl3;
using Org.Mentalis.Security.Ssl.Tls1;
using Org.Mentalis.Security.Cryptography;

namespace Org.Mentalis.Security.Ssl.Shared {
	internal sealed class CompatibilityLayer {
		public CompatibilityLayer(SocketController controller, SecurityOptions options) {
			m_Buffer = new byte[0];
			m_MinVersion = GetMinProtocol(options.Protocol);
			m_MaxVersion = GetMaxProtocol(options.Protocol);
			if (m_MinVersion.GetVersionInt() == 30) { // SSL 3.0
				if (options.Entity == ConnectionEnd.Client)
					m_MinLayer = new RecordLayer(controller, new Ssl3ClientHandshakeLayer(null, options));
				else
					m_MinLayer = new RecordLayer(controller, new Ssl3ServerHandshakeLayer(null, options));
			} else { // TLS 1.0
				if (options.Entity == ConnectionEnd.Client)
					m_MinLayer = new RecordLayer(controller, new Tls1ClientHandshakeLayer(null, options));
				else
					m_MinLayer = new RecordLayer(controller, new Tls1ServerHandshakeLayer(null, options));
			}
			m_MinLayer.HandshakeLayer.RecordLayer = m_MinLayer;
			m_Options = options;
		}
		public byte[] GetClientHello() {
			if (m_Hello == null)
				m_Hello = m_MinLayer.GetControlBytes(ControlType.ClientHello);
			return m_Hello;
		}
		// return null if more bytes are needed
		// throws an SslException if the bytes are invalid
		// returns a RecordLayer instance if the method completed successfully
		public CompatibilityResult ProcessHello(byte[] bytes, int offset, int size) {
			if (m_Options.Entity == ConnectionEnd.Client)
				return ProcessServerHello(bytes, offset, size);
			else 
				return ProcessClientHello(bytes, offset, size);
		}
		private CompatibilityResult ProcessServerHello(byte[] bytes, int offset, int size) {
			byte[] temp = new byte[m_Buffer.Length + size];
			Array.Copy(m_Buffer, 0, temp, 0, m_Buffer.Length);
			Array.Copy(bytes, offset, temp, m_Buffer.Length, size);
			if (IsInvalidSsl3Hello(temp))
				throw new SslException(AlertDescription.HandshakeFailure, "The server hello message uses a protocol that was not recognized.");
			if (m_Buffer.Length + size < 11) { // not enough bytes
				m_Buffer = temp;
				return new CompatibilityResult(null, new SslRecordStatus(SslStatus.MessageIncomplete, null, null));
			}
			ProtocolVersion pv = new ProtocolVersion(temp[9], temp[10]);
			if (SupportsProtocol(m_Options.Protocol, pv)) {
				if (m_MinLayer.HandshakeLayer.GetVersion().GetVersionInt() != pv.GetVersionInt()) {
					if (pv.GetVersionInt() == 30) { // SSL 3.0
						m_MinLayer.HandshakeLayer = new Ssl3ClientHandshakeLayer(m_MinLayer.HandshakeLayer);
					} else { // TLS 1.0
						m_MinLayer.HandshakeLayer = new Tls1ClientHandshakeLayer(m_MinLayer.HandshakeLayer);
					}
				}
				return new CompatibilityResult(m_MinLayer, m_MinLayer.ProcessBytes(temp, 0, temp.Length));
			} else {
				throw new SslException(AlertDescription.HandshakeFailure, "The client and server could not agree on the protocol version to use.");
			}
		}
		private bool IsInvalidSsl3Hello(byte[] buffer) { // also works for TLS1 hellos
			return (buffer.Length > 0 && buffer[0] != 22)
						|| (buffer.Length > 1 && buffer[1] != 3)
						|| (buffer.Length > 2 && buffer[2] != 0 && buffer[2] != 1);
			
		}
		private bool IsInvalidSsl2Hello(byte[] buffer) {
			if (buffer.Length < 6)
				return false;
			int offset;
			if ((buffer[0] & 0x80) != 0) { // no padding
				offset = 2;
			} else { // padding
				offset = 3;
			}
			return buffer[offset] != 1 || buffer[offset+1] != 3 || (buffer[offset+2] != 0 && buffer[offset+2] != 1);
		}
		private bool IsSsl2HelloComplete(byte[] buffer) {
			if (buffer.Length < 3)
				return false;
			if ((buffer[0] & 0x80) != 0) { // no padding
				return buffer.Length == (((buffer[0] & 0x7f) << 8) | buffer[1] + 2);
			} else { // padding
				return buffer.Length == (((buffer[0] & 0x3f) << 8) | buffer[1] + 3);
			}
		}
		private byte[] ExtractSsl2Content(byte[] buffer) {
			byte[] ret;
			if ((buffer[0] & 0x80) != 0) { // no padding
				ret = new byte[buffer.Length - 2];
			} else { // padding
				ret = new byte[buffer.Length - 3];
			}
			Array.Copy(buffer, buffer.Length - ret.Length, ret, 0, ret.Length);
			return ret;
		}
		private ProtocolVersion ExtractSsl2Version(byte[] buffer) {
			if ((buffer[0] & 0x80) != 0) { // no padding
				return new ProtocolVersion(buffer[3], buffer[4]);
			} else { // padding
				return new ProtocolVersion(buffer[4], buffer[5]);
			}
		}
		private CompatibilityResult ProcessClientHello(byte[] bytes, int offset, int size) {
			byte[] temp = new byte[m_Buffer.Length + size];
			Array.Copy(m_Buffer, 0, temp, 0, m_Buffer.Length);
			Array.Copy(bytes, offset, temp, m_Buffer.Length, size);
			if (IsInvalidSsl3Hello(temp) && IsInvalidSsl2Hello(temp)) // SSL2 hello
				throw new SslException(AlertDescription.HandshakeFailure, "The client hello message uses a protocol that was not recognized.");
			if (m_Buffer.Length + bytes.Length < 11 || (IsInvalidSsl3Hello(temp) && !IsSsl2HelloComplete(temp))) { // not enough bytes
				m_Buffer = temp;
				return new CompatibilityResult(null, new SslRecordStatus(SslStatus.MessageIncomplete, null, null));
			}
			ProtocolVersion pv;
			if (!IsInvalidSsl3Hello(temp))
				pv = new ProtocolVersion(temp[9], temp[10]);
			else
				pv = ExtractSsl2Version(temp);

			if (pv.GetVersionInt() > m_MaxVersion.GetVersionInt())
				pv = m_MaxVersion;
			if (SupportsProtocol(m_Options.Protocol, pv)) {
				if (m_MinLayer.HandshakeLayer.GetVersion().GetVersionInt() != pv.GetVersionInt()) {
					if (pv.GetVersionInt() == 30) { // SSL 3.0
						m_MinLayer.HandshakeLayer = new Ssl3ServerHandshakeLayer(m_MinLayer.HandshakeLayer);
					} else { // TLS 1.0
						m_MinLayer.HandshakeLayer = new Tls1ServerHandshakeLayer(m_MinLayer.HandshakeLayer);
					}
				}
				if (!IsInvalidSsl3Hello(temp)) {
					return new CompatibilityResult(m_MinLayer, m_MinLayer.ProcessBytes(temp, 0, temp.Length));
				} else {
					return new CompatibilityResult(m_MinLayer, m_MinLayer.ProcessSsl2Hello(ExtractSsl2Content(temp)));
				}
			} else {
				throw new SslException(AlertDescription.HandshakeFailure, "The client and server could not agree on the protocol version to use.");
			}
		}
		public static bool SupportsSsl3(SecureProtocol protocol) {
			return ((int)protocol & (int)SecureProtocol.Ssl3) != 0;
		}
		public static bool SupportsTls1(SecureProtocol protocol) {
			return ((int)protocol & (int)SecureProtocol.Tls1) != 0;
		}
		public static bool SupportsProtocol(SecureProtocol protocol, ProtocolVersion pv) {
			if (pv.GetVersionInt() == 30)
				return SupportsSsl3(protocol);
			else
				return SupportsTls1(protocol);
		}
		public static ProtocolVersion GetMinProtocol(SecureProtocol protocol) {
			if (SupportsSsl3(protocol))
				return new ProtocolVersion(3, 0);
			else
				return new ProtocolVersion(3, 1);
		}
		public static ProtocolVersion GetMaxProtocol(SecureProtocol protocol) {
			if (SupportsTls1(protocol))
				return new ProtocolVersion(3, 1);
			else
				return new ProtocolVersion(3, 0);
		}
		private byte[] m_Hello;
		private ProtocolVersion m_MinVersion;
		private ProtocolVersion m_MaxVersion;
		private RecordLayer m_MinLayer;
		private SecurityOptions m_Options;
		private byte[] m_Buffer;
	}
}
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

namespace Org.Mentalis.Security.Ssl.Shared {
	internal class RecordMessage {
		public RecordMessage(MessageType messageType, ContentType contentType, ProtocolVersion version, byte[] bytes) {
			this.messageType = messageType;
			this.contentType = contentType;
			this.version = version;
			if (bytes != null)
				this.fragment = bytes;
			else
				this.fragment = new byte[0];
			this.length = (ushort)this.fragment.Length;
		}
		public RecordMessage(byte[] bytes, int offset) {
			if (bytes == null)
				throw new ArgumentNullException();
			if (offset < 0 || offset >= bytes.Length)
				throw new ArgumentException();
			this.messageType = MessageType.Encrypted;
			this.contentType = (ContentType)bytes[offset];
			this.version = new ProtocolVersion(bytes[offset + 1], bytes[offset + 2]);
			this.length = (ushort)(bytes[offset + 3] * 256 + bytes[offset + 4]);
			this.fragment = new byte[this.length];
			Array.Copy(bytes, offset + 5, this.fragment, 0, this.length);
		}
		public byte[] ToBytes() {
			byte[] ret = new byte[fragment.Length + 5];
			ret[0] = (byte)contentType;
			ret[1] = version.major;
			ret[2] = version.minor;
			ret[3] = (byte)(length / 256);
			ret[4] = (byte)(length % 256);
			Array.Copy(fragment, 0, ret, 5, fragment.Length);
			return ret;
		}
		public MessageType messageType;
		public ContentType contentType;
		public ProtocolVersion version;
		public ushort length;
		public byte[] fragment;
	}
}
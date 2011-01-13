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
	internal abstract class CompressionAlgorithm {
		public CompressionAlgorithm() {}
		public byte[] Compress(byte[] data) {
			return data;
		}
		public byte[] Decompress(byte[] data) {
			return data;
		}
		public static SslAlgorithms GetCompressionAlgorithm(byte[] algos, SslAlgorithms allowed) {
			for(int i = 0; i < algos.Length; i++) {
				if (algos[i] == 0)
					return SslAlgorithms.NULL_COMPRESSION;
			}
			throw new SslException(AlertDescription.HandshakeFailure, "No compression method matches the available compression methods.");
		}
		public static byte GetAlgorithmByte(SslAlgorithms algorithm) {
			switch(algorithm) {
				case SslAlgorithms.NULL_COMPRESSION:
					return 0;
				default:
					return 0; // perhaps throw error?
			}
		}
		public static SslAlgorithms GetCompressionAlgorithmType(byte[] buffer, int offset) {
			switch(buffer[offset]) {
				case 0:
				default:
					return SslAlgorithms.NULL_COMPRESSION;
			}
		}
		public static byte[] GetCompressionAlgorithmBytes(SslAlgorithms algorithm) {
			return new byte[]{0};
		}
	}
}
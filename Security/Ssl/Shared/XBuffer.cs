using System;
using System.IO;

namespace Org.Mentalis.Security.Ssl.Shared {
	/// <summary>
	/// Creates a stream whose backing store is memory.
	/// </summary>
	/// <remarks>This class is created by Kevin Knoop.</remarks>
	internal class XBuffer : MemoryStream {
		/// <summary>
		/// Initializes a new instance of the XBuffer class with an expandable capacity initialized to zero.
		/// </summary>
		public XBuffer() : base() {
			//
		}
		/// <summary>
		/// Removes a number of leading bytes from the buffer.
		/// </summary>
		/// <param name="aByteCount">The number of bytes to remove.</param>
		/// <exception cref="ArgumentException"><paramref name="aByteCount"/> is invalid.</exception>
		public void RemoveXBytes(int aByteCount) {
			if (aByteCount > Length) {
				throw new ArgumentException("Not enough data in buffer");
			}
			if (aByteCount == Length) {
				SetLength(0);
			} else {
				byte[] buff = GetBuffer();
				Array.Copy(buff, aByteCount, buff, 0, (int)Length-aByteCount);
				SetLength(Length-aByteCount);
			}
		}
	}
}
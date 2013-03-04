using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portable.Utils.Text
{
    /// <summary>
    /// Silverlight doesn't have an ASCII encoder, so here is one:
    /// </summary>
    public class AsciiEncoding : System.Text.Encoding
    {
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }
        public override byte[] GetBytes(char[] chars)
        {
            return base.GetBytes(chars);
        }
        public override int GetCharCount(byte[] bytes)
        {
            return bytes.Length;
        }
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                bytes[byteIndex + i] = (byte)chars[charIndex + i];
            }
            return charCount;
        }
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                chars[charIndex + i] = (char)bytes[byteIndex + i];
            }
            return byteCount;
        }
    }
}

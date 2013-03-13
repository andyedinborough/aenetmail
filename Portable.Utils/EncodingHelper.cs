using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Portable.Utils.Text;

namespace Portable.Utils
{
    public static class EncodingHelper
    {
        private static Encoding _asciiEncoding;
        private static Iso88591Encoding _iso88591;
        private static Iso88592Encoding _iso88592;

        public static Encoding GetDefault()
        {
            return Encoding.UTF8;
        }

        public static Encoding GetASCII()
        {
            return _asciiEncoding ?? (_asciiEncoding = new AsciiEncoding());
        }

        public static string GetString(this Encoding encoding, byte[] array)
        {
            return encoding.GetString(array, 0, array.Length);
        }

        public static Encoding GetIso88591()
        {
            return _iso88591 ?? (_iso88591 = new Iso88591Encoding());
        }

        public static Encoding GetIso88592()
        {
            return _iso88592 ?? (_iso88592 = new Iso88592Encoding());
        }

        public static byte[] EncodeUTF7(string nonAsciiString)
        {
            throw new NotImplementedException();
        }

        public static string DecodeUTF7(string modifiedBase64)
        {
            if (modifiedBase64.StartsWith("+"))
                modifiedBase64 = modifiedBase64.Substring(1);

            Int16 curr = 0, overflow = 0;
            short bits = 0;
            string result = string.Empty;

            for (int i = 0; i < modifiedBase64.Length; ++i)
            {
                var index = (Int16)ModifiedBase64.IndexOf(modifiedBase64[i]);
                if (bits + 6 < 16)
                {
                    curr = (Int16)(curr << 6 | index);
                    bits += 6;
                }
                else
                {
                    var remaining = (16 - bits);
                    var leftover = (6 - remaining);

                    curr = (Int16)(curr << remaining | (index >> leftover));
                    result += Convert.ToChar(curr);
                    bits = (Int16)(index & (powers[leftover]));
                }
            }

            return result;
        }

        private static Int16[] powers = new Int16[] { 1, 3, 7, 15, 31, 63 };
        private const string ModifiedBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    }
}

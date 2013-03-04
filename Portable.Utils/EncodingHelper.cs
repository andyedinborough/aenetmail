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

        public static Encoding GetUTF7()
        {
            throw new System.NotImplementedException();
        }

        public static Encoding GetIso88591()
        {
            return _iso88591 ?? (_iso88591 = new Iso88591Encoding());
        }

        public static Encoding GetIso88592()
        {
            return _iso88592 ?? (_iso88592 = new Iso88592Encoding());
        }
    }
}

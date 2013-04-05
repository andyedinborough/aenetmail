using System.Text;

namespace AE.Net.Mail
{
#if !WINDOWS_PHONE
    public static class EncodingHelper
    {
        public static Encoding GetASCII()
        {
            return Encoding.ASCII;
        }
        public static Encoding GetDefault()
        {
            return Encoding.Default;
        }
        public static Encoding GetUTF7()
        {
            return Encoding.UTF7;
        }
    }
#endif
}

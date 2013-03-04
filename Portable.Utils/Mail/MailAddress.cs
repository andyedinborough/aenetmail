namespace Portable.Utils.Mail
{
    public class MailAddress
    {
        private readonly string _address;

        public MailAddress(string address)
        {
            _address = address;
        }

        public override string ToString()
        {
            return _address;
        }
    }
}

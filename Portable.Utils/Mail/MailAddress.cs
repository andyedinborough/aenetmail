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

        public string DisplayName
        {
            get
            {
                return _address.Contains("<") ? _address.Split('<')[0].Trim() : _address;
            }
        }
    }
}

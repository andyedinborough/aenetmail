using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AE.Net.Mail
{
    [Serializable()]
    public class ImapClientException : Exception
    {
        public ImapClientException() : base() { }
        public ImapClientException(string message) : base(message) { }
        public ImapClientException(string message, Exception inner) : base(message, inner) { }

        protected ImapClientException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) { }
    }
}

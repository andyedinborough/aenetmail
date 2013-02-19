using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Portable.Utils.Mail
{
    public class MailMessage
    {
        public string Subject { get; set; }

        public MailAddress Sender { get; set; }

        public string Body { get; set; }

        public bool IsBodyHtml { get; set; }

        public MailAddress From { get; set; }

        public List<MailAddress> Bcc { get; set; }

        public MailPriority Priority { get; set; }

        public List<MailAddress> ReplyToList { get; set; }

        public List<MailAddress> To { get; set; }
    }
}

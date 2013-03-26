using System;

namespace AE.Net.Mail.Imap {
    public class MessageEventArgs : EventArgs {
        public virtual int MessageCount { get; set; }
        internal IImapClient Client { get; set; }
    }
}

using System;

namespace AE.Net.Mail.Imap {
    public class MessageEventArgs : EventArgs {
        public int MessageCount { get; set; }
        internal ImapClient Client { get; set; }
    }
}

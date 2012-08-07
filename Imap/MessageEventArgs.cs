using System;

namespace AE.Net.Mail.Imap {
  /// <summary>
  /// Provides data for the NewMessage event.
  /// </summary>
  public class MessageEventArgs : EventArgs {
    /// <summary>
    /// The total number of messages in the mailbox
    /// </summary>
    public int MessageCount { get; set; }
    internal ImapClient Client { get; set; }
  }
}

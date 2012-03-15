using System;

namespace AE.Net.Mail {
  public interface IMailClient : IDisposable {
    int GetMessageCount();
        MailMessage GetMessage(int index);
        MailMessage GetMessage(int index, bool headersonly);
        MailMessage GetMessage(string uid);
        MailMessage GetMessage(string uid, bool headersonly);
    void DeleteMessage(string uid);
    void DeleteMessage(AE.Net.Mail.MailMessage msg);
    void Disconnect();
  }
}

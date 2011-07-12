using System;

namespace AE.Net.Mail {
    public interface IMailClient : IDisposable {
        int GetMessageCount();
        MailMessage GetMessage(int index, bool headersonly = false);
        MailMessage GetMessage(string uid, bool headersonly = false);
        void DeleteMessage(string uid);
    }
}

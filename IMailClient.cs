
namespace AE.Net.Mail {
    public interface IMailClient {
        int GetMessageCount();
        MailMessage GetMessage(int index, bool headersonly = false);
    }
}

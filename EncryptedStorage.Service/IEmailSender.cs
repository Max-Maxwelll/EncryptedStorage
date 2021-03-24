using System.Threading.Tasks;

namespace EncryptedStorage.Service
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}

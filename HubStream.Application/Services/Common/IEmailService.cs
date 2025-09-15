using System.Threading.Tasks;

namespace Application.Interfaces.Services.Common
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendConfirmationEmailAsync(string to, string userId, string token);
        Task SendPasswordResetEmailAsync(string to, string token);
    }
}
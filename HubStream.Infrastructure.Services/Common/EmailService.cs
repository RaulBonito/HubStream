using Application.Interfaces.Services.Common;
using Microsoft.Extensions.Configuration;
using MailKit.Security;
using MimeKit;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKitSmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace HubStream.Infrastructure.Services.Common
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:FromName"], _configuration["EmailSettings:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            await SendAsync(message);
        }

        public async Task SendConfirmationEmailAsync(string to, string userId, string token)
        {
            var confirmationLink = GenerateConfirmationLink(userId, token);
            var subject = "Confirma tu cuenta";
            var body = $"<p>Para confirmar tu cuenta, haz clic <a href='{confirmationLink}'>aquí</a>.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string token)
        {
            var resetLink = GeneratePasswordResetLink(token);
            var subject = "Restablece tu contraseña";
            var body = $"<p>Para restablecer tu contraseña, haz clic <a href='{resetLink}'>aquí</a>.</p>";

            await SendEmailAsync(to, subject, body);
        }

        private async Task SendAsync(MimeMessage message)
        {
            using var smtp = new MailKitSmtpClient();
            try
            {
                var host = _configuration["EmailSettings:SmtpHost"];
                var port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var user = _configuration["EmailSettings:SmtpUser"];
                var pass = _configuration["EmailSettings:SmtpPass"];

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable);

                if (!string.IsNullOrWhiteSpace(user))
                {
                    await smtp.AuthenticateAsync(user, pass);
                }

                await smtp.SendAsync(message);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        private string GenerateConfirmationLink(
            string userId,
            string token)
        {
            var baseUrl = _configuration["AppSettings:FrontendUrl"]
                          ?? "http://localhost:4200";

            // 1) Codifica el token en Base64‑URL usando la nueva API
            string encodedToken = Base64Url.EncodeToString(
                Encoding.UTF8.GetBytes(token));   // un allocation pequeño y listo

            // 2) Construye la URL
            return $"{baseUrl}/confirm-account" +
                   $"?userId={Uri.EscapeDataString(userId)}" +
                   $"&token={encodedToken}";
        }





        private string GeneratePasswordResetLink(string token)
        {
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:4200";
            return $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        }
    }
}

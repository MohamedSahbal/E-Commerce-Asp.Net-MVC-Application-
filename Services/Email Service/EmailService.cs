using ECommerce_Application.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerce_Application.Services.Email_Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            settings = options.Value;
        }

        public async Task SendOtpAsync(string email, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(settings.DisplayName, settings.Email));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = "Dreams Shop Password Reset";

            message.Body = new TextPart("html")
            {
                Text = $"<h2>Your OTP is</h2><h1>{otp}</h1><p>Valid for 5 minutes.</p>"
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                settings.Host,
                settings.Port,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                settings.Email,
                settings.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}

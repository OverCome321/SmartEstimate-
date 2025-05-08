using Bl.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Bl
{
    public class EmailServiceBL : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;

        public EmailServiceBL(IConfiguration configuration)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings");
            _smtpServer = smtpSettings["SmtpServer"];
            _smtpPort = int.Parse(smtpSettings["SmtpPort"]);
            _smtpUser = smtpSettings["SmtpUser"];
            _smtpPass = smtpSettings["SmtpPass"];
            _fromEmail = smtpSettings["FromEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("SmartEstimate", _fromEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart("plain") { Text = body };

                using var client = new SmtpClient();

                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                try
                {
                    await client.AuthenticateAsync(_smtpUser, _smtpPass);
                    await client.SendAsync(email);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке email: {ex.Message}", ex);
            }
        }
    }
}
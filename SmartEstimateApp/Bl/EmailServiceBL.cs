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
        private readonly string _fromName;
        private readonly string _appName;
        private readonly string _domain;

        public EmailServiceBL(IConfiguration configuration)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings");
            _smtpServer = smtpSettings["SmtpServer"] ?? throw new ArgumentNullException("SmtpServer configuration is missing");
            _smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
            _smtpUser = smtpSettings["SmtpUser"] ?? throw new ArgumentNullException("SmtpUser configuration is missing");
            _smtpPass = smtpSettings["SmtpPass"] ?? throw new ArgumentNullException("SmtpPass configuration is missing");
            _fromEmail = smtpSettings["FromEmail"] ?? throw new ArgumentNullException("FromEmail configuration is missing");
            _fromName = smtpSettings["FromName"] ?? configuration.GetSection("AppSettings")["AppName"] ?? "SmartEstimate";
            _appName = configuration.GetSection("AppSettings")["AppName"] ?? "SmartEstimate";
            _domain = _fromEmail.Split('@')[1];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Создаем новое сообщение
                var email = new MimeMessage();

                // Настраиваем отправителя правильно
                email.From.Add(new MailboxAddress(_fromName, _fromEmail));
                email.Sender = new MailboxAddress(_fromName, _fromEmail); // Важно для Gmail
                email.ReplyTo.Add(new MailboxAddress(_fromName, _fromEmail));

                // Добавляем получателя
                email.To.Add(MailboxAddress.Parse(to));

                // Устанавливаем тему
                email.Subject = subject;

                // Добавляем заголовки, которые улучшают доставляемость
                var messageId = $"{Guid.NewGuid().ToString("N")}@{_domain}";
                email.MessageId = $"<{messageId}>";
                email.Headers.Add("X-Mailer", $"{_appName} Application");
                email.Headers.Add("X-Auto-Response-Suppress", "OOF, DR, RN, NRN, AutoReply");
                email.Headers.Add("Auto-Submitted", "auto-generated");
                email.Headers.Add("X-Priority", "3"); // Нормальный приоритет (не важный)
                email.Headers.Add("Importance", "normal");
                email.Headers.Add("Precedence", "bulk");
                email.Headers.Add("X-Entity-Ref-ID", messageId);

                // Указываем дату отправки
                email.Date = DateTimeOffset.Now;

                // Создаем тело письма с HTML и текстовой версией
                var builder = new BodyBuilder();

                // HTML версия
                builder.HtmlBody = body;

                // Текстовая версия (обязательно для улучшения доставляемости)
                builder.TextBody = HtmlToPlainText(body);

                // Финализируем тело письма
                email.Body = builder.ToMessageBody();

                // Отправляем письмо
                using var client = new SmtpClient();

                // Настраиваем таймаут и другие параметры
                client.Timeout = 30000; // 30 секунд
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Для отладки

                // Подключаемся к серверу
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);

                try
                {
                    // Аутентификация
                    await client.AuthenticateAsync(_smtpUser, _smtpPass);

                    // Отправка письма
                    await client.SendAsync(email);
                }
                finally
                {
                    // Отключаемся от сервера
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке email: {ex.Message}", ex);
            }
        }

        // Улучшенный конвертер из HTML в текст
        private string HtmlToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Заменяем некоторые элементы для лучшего форматирования
            var text = html;

            // Заменяем BR на перенос строки
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<br\s*\/?>", "\r\n");

            // Заменяем P на перенос строки с пустой строкой
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<\/p>", "\r\n\r\n");

            // Заменяем заголовки
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<h[1-6][^>]*>", "\r\n\r\n");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<\/h[1-6]>", "\r\n\r\n");

            // Удаляем все теги HTML
            text = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]*>", "");

            // Заменяем множественные пробелы на один
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Заменяем множественные переносы строк
            text = System.Text.RegularExpressions.Regex.Replace(text, @"(\r\n){3,}", "\r\n\r\n");

            // Удаляем все непечатные символы
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\S\r\n]+", " ");

            // HTML-декодирование для специальных символов
            text = System.Net.WebUtility.HtmlDecode(text);

            return text.Trim();
        }
    }
}
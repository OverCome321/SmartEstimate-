using Bl.Interfaces;
using Bl.Managers;
using Entities;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Bl
{
    public class EmailVerificationServiceBL
    {
        private readonly IEmailService _emailSender;
        private readonly Random _random = new Random();
        private readonly IConfiguration _configuration;

        public EmailVerificationServiceBL(IEmailService emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Отправляет код подтверждения на указанный email
        /// </summary>
        public async Task<string> SendVerificationCodeAsync(string email, VerificationPurpose purpose)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email не может быть пустым.", nameof(email));

            // Генерация 6-значного кода
            string code = GenerateVerificationCode();

            // Получаем название приложения из конфигурации
            var appName = _configuration["AppSettings:AppName"] ?? "SmartEstimate";
            var companyName = _configuration["AppSettings:CompanyName"] ?? "Умный счетчик смет";

            // Сохраняем код в хранилище с указанием цели
            string sessionId = VerificationCodeStore.StoreCode(email, code, purpose);

            // Формируем заголовок в зависимости от цели верификации
            string subject = purpose switch
            {
                VerificationPurpose.Login => $"{code} - Ваш код для входа в {appName}",
                VerificationPurpose.PasswordReset => $"{code} - Ваш код для восстановления пароля в {appName}",
                VerificationPurpose.Registration => $"{code} - Ваш код для регистрации в {appName}",
                _ => $"{code} - Ваш код подтверждения для {appName}"
            };

            // Отправляем красивый HTML email с кодом
            await _emailSender.SendEmailAsync(
                email,
                subject,
                GenerateEmailBody(code, purpose, appName, companyName)
            );

            return sessionId;
        }

        /// <summary>
        /// Проверяет код подтверждения
        /// </summary>
        public bool VerifyCode(string email, string code, VerificationPurpose purpose, string sessionId = null)
        {
            return VerificationCodeStore.VerifyCode(email, code, purpose, sessionId);
        }

        /// <summary>
        /// Отменяет действие кода подтверждения
        /// </summary>
        public void InvalidateCode(string email, VerificationPurpose purpose)
        {
            VerificationCodeStore.InvalidateCode(email, purpose);
        }

        /// <summary>
        /// Генерирует 6-значный код подтверждения
        /// </summary>
        private string GenerateVerificationCode() => _random.Next(100000, 999999).ToString();

        /// <summary>
        /// Генерирует красивый HTML шаблон для email с кодом подтверждения
        /// </summary>
        private string GenerateEmailBody(string code, VerificationPurpose purpose, string appName, string companyName)
        {
            string actionText = purpose switch
            {
                VerificationPurpose.Login => "входа в аккаунт",
                VerificationPurpose.PasswordReset => "восстановления пароля",
                VerificationPurpose.Registration => "регистрации аккаунта",
                _ => "подтверждения"
            };

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ru\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\">");
            sb.AppendLine($"    <title>Код подтверждения {code}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        @media only screen and (max-width: 600px) {");
            sb.AppendLine("            .container {");
            sb.AppendLine("                width: 100% !important;");
            sb.AppendLine("                padding: 0 !important;");
            sb.AppendLine("            }");
            sb.AppendLine("            .content {");
            sb.AppendLine("                padding: 15px !important;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        body {");
            sb.AppendLine("            font-family: 'Segoe UI', Arial, sans-serif;");
            sb.AppendLine("            line-height: 1.6;");
            sb.AppendLine("            color: #333333;");
            sb.AppendLine("            margin: 0;");
            sb.AppendLine("            padding: 0;");
            sb.AppendLine("            background-color: #f9f9f9;");
            sb.AppendLine("            -webkit-font-smoothing: antialiased;");
            sb.AppendLine("        }");
            sb.AppendLine("        .container {");
            sb.AppendLine("            max-width: 600px;");
            sb.AppendLine("            margin: 20px auto;");
            sb.AppendLine("            background-color: #ffffff;");
            sb.AppendLine("            border-radius: 8px;");
            sb.AppendLine("            overflow: hidden;");
            sb.AppendLine("            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);");
            sb.AppendLine("        }");
            sb.AppendLine("        .header {");
            sb.AppendLine("            background-color: #2e58ff;");
            sb.AppendLine("            padding: 20px;");
            sb.AppendLine("            text-align: center;");
            sb.AppendLine("        }");
            sb.AppendLine("        .header img {");
            sb.AppendLine("            max-height: 50px;");
            sb.AppendLine("        }");
            sb.AppendLine("        .header h1 {");
            sb.AppendLine("            color: white;");
            sb.AppendLine("            margin: 0;");
            sb.AppendLine("            font-size: 22px;");
            sb.AppendLine("            font-weight: 500;");
            sb.AppendLine("        }");
            sb.AppendLine("        .content {");
            sb.AppendLine("            padding: 30px;");
            sb.AppendLine("        }");
            sb.AppendLine("        .code-container {");
            sb.AppendLine("            background-color: #f5f7ff;");
            sb.AppendLine("            border-radius: 6px;");
            sb.AppendLine("            padding: 20px 15px;");
            sb.AppendLine("            text-align: center;");
            sb.AppendLine("            margin: 25px 0;");
            sb.AppendLine("            border: 1px solid #e1e5ff;");
            sb.AppendLine("        }");
            sb.AppendLine("        .verification-code {");
            sb.AppendLine("            font-size: 36px;");
            sb.AppendLine("            font-weight: bold;");
            sb.AppendLine("            letter-spacing: 6px;");
            sb.AppendLine("            color: #2e58ff;");
            sb.AppendLine("            margin: 0;");
            sb.AppendLine("            font-family: monospace;");
            sb.AppendLine("        }");
            sb.AppendLine("        .footer {");
            sb.AppendLine("            background-color: #f5f7ff;");
            sb.AppendLine("            padding: 20px;");
            sb.AppendLine("            text-align: center;");
            sb.AppendLine("            font-size: 12px;");
            sb.AppendLine("            color: #666666;");
            sb.AppendLine("        }");
            sb.AppendLine("        .warning {");
            sb.AppendLine("            color: #e74c3c;");
            sb.AppendLine("            font-size: 14px;");
            sb.AppendLine("            margin-top: 20px;");
            sb.AppendLine("            padding: 10px;");
            sb.AppendLine("            background: #ffebee;");
            sb.AppendLine("            border-radius: 4px;");
            sb.AppendLine("        }");
            sb.AppendLine("        p {");
            sb.AppendLine("            margin-top: 0;");
            sb.AppendLine("            margin-bottom: 16px;");
            sb.AppendLine("        }");
            sb.AppendLine("        .btn {");
            sb.AppendLine("            display: inline-block;");
            sb.AppendLine("            background-color: #2e58ff;");
            sb.AppendLine("            color: white;");
            sb.AppendLine("            text-decoration: none;");
            sb.AppendLine("            padding: 10px 20px;");
            sb.AppendLine("            border-radius: 4px;");
            sb.AppendLine("            font-weight: 500;");
            sb.AppendLine("            margin-top: 20px;");
            sb.AppendLine("        }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine($"            <h1>{appName}</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"content\">");
            sb.AppendLine("            <p>Здравствуйте!</p>");
            sb.AppendLine($"            <p>Для {actionText} в системе {appName} используйте следующий код подтверждения:</p>");
            sb.AppendLine("            <div class=\"code-container\">");
            sb.AppendLine($"                <p class=\"verification-code\">{code}</p>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <p>Код действителен в течение 5 минут.</p>");
            sb.AppendLine($"            <p>Если вы не запрашивали {actionText} в системе {appName}, пожалуйста, проигнорируйте это письмо или свяжитесь с нашей службой поддержки.</p>");
            sb.AppendLine("            <p class=\"warning\">Не сообщайте этот код никому, включая сотрудников технической поддержки.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine($"            <p>Это автоматическое сообщение, не отвечайте на него.<br/>&copy; {DateTime.Now.Year} {companyName}. Все права защищены.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
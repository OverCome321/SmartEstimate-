using Bl.Interfaces;
using Bl.Managers;

namespace Bl
{
    public class EmailVerificationServiceBL
    {
        private readonly IEmailService _emailSender;
        private readonly Random _random = new Random();

        public EmailVerificationServiceBL(IEmailService emailSender)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        /// <summary>
        /// Отправляет код подтверждения на указанный email
        /// </summary>
        public async Task SendVerificationCodeAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email не может быть пустым.", nameof(email));

            // Генерация 6-значного кода
            string code = GenerateVerificationCode();

            // Сохраняем код в хранилище
            VerificationCodeStore.StoreCode(email, code);

            // Отправляем код на email
            await _emailSender.SendEmailAsync(
                email,
                "Код подтверждения",
                $"Ваш код подтверждения: {code}. Действителен в течение 5 минут."
            );
        }
        /// <summary>
        /// Проверяет код подтверждения
        /// </summary>
        public bool VerifyCode(string email, string code) => VerificationCodeStore.VerifyCode(email, code);

        /// <summary>
        /// Генерирует 6-значный код подтверждения
        /// </summary>
        private string GenerateVerificationCode() => _random.Next(100000, 999999).ToString();

    }
}
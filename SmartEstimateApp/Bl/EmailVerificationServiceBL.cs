using Bl.Interfaces;
using Bl.Managers;

namespace Bl
{
    public class EmailVerificationServiceBL
    {
        private readonly IEmailService _emailService;

        public EmailVerificationServiceBL(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendVerificationCodeAsync(string email)
        {
            try
            {
                string code = new Random().Next(100000, 999999).ToString();

                VerificationCodeStore.StoreCode(email, code);

                string subject = "Код подтверждения для входа";
                string body = $"Ваш код подтверждения: {code}. Код действителен 5 минут.";
                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке кода подтверждения: {ex.Message}", ex);
            }
        }

        public bool VerifyCode(string email, string code)
        {
            return VerificationCodeStore.VerifyCode(email, code);
        }
    }
}
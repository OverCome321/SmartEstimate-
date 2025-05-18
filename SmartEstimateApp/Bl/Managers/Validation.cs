using Bl.DI;
using Entities;

namespace Bl.Managers
{
    public class Validation
    {
        /// <summary>
        /// Проверяет формат email
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True, если формат email валиден; иначе false</returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Проверяет формат телефона (базовая валидация)
        /// </summary>
        /// <param name="phone">Телефон для проверки</param>
        /// <returns>True, если формат телефона валиден; иначе false</returns>
        public static bool IsValidPhone(string phone)
        {
            // Базовая валидация телефона: допускаются цифры, +, -, пробелы, минимальная длина 7
            return !string.IsNullOrEmpty(phone) &&
                   phone.Length >= 7 &&
                   phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ');
        }
        /// <summary>
        /// Проверяет сложность пароля согласно настройкам
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        /// <exception cref="ArgumentException">Выбрасывается, если пароль не соответствует требованиям</exception>
        public static void ValidatePassword(string password, BusinessLogicOptions options)
        {
            if (password.Length < options.MinPasswordLength)
            {
                throw new ArgumentException(
                    string.Format(ErrorMessages.PasswordTooShort, options.MinPasswordLength),
                    nameof(password));
            }

            if (options.RequireComplexPassword)
            {
                bool hasDigit = password.Any(char.IsDigit);
                bool hasLetter = password.Any(char.IsLetter);
                bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

                if (!hasDigit || !hasLetter || !hasSpecial)
                {
                    throw new ArgumentException(
                        ErrorMessages.ComplexPasswordRequired,
                        nameof(password));
                }
            }
        }
    }
}

using Bl.DI;
using Bl.Interfaces;
using Common.Convert;
using Common.Search;
using Common.Security;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Options;

namespace Bl
{
    /// <summary>
    /// Бизнес-логика для работы с пользователями
    /// </summary>
    public class UserBL : IUserBL
    {
        private readonly IUserDal _userDal;
        private readonly BusinessLogicOptions _options;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserBL"/>
        /// </summary>
        /// <param name="userDal">Data Access Layer для пользователей</param>
        /// <param name="options">Опции бизнес-логики (опционально)</param>
        public UserBL(IUserDal userDal, IOptions<BusinessLogicOptions> options = null)
        {
            _userDal = userDal ?? throw new ArgumentNullException(nameof(userDal));
            _options = options?.Value ?? new BusinessLogicOptions();
        }

        public async Task<Guid> AddOrUpdateAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Email))
                throw new ArgumentException(ErrorMessages.EmailEmpty, nameof(entity.Email));

            if (!IsValidEmail(entity.Email))
                throw new ArgumentException(ErrorMessages.EmailInvalidFormat, nameof(entity.Email));

            if (entity.Role == null || entity.Role.Id == Guid.Empty)
                throw new ArgumentException(ErrorMessages.RoleNotSpecified, nameof(entity.Role));

            if (string.IsNullOrEmpty(entity.PasswordHash))
                throw new ArgumentException(ErrorMessages.PasswordEmpty, nameof(entity.PasswordHash));

            if (_options.EnableExtendedValidation)
                ValidatePassword(entity.PasswordHash);

            if (await _userDal.ExistsAsync(entity.Email))
                throw new InvalidOperationException(string.Format(ErrorMessages.EmailAlreadyExists, entity.Email));

            entity.PasswordHash = PasswordHasher.HashPassword(entity.PasswordHash);

            if (entity.Id == Guid.Empty)
            {
                entity.CreatedAt = DateTime.Now;
            }

            return await _userDal.AddOrUpdateAsync(entity);
        }
        public Task<bool> ExistsAsync(Guid id)
        {
            return _userDal.ExistsAsync(id);
        }
        public Task<bool> ExistsAsync(string email)
        {
            return _userDal.ExistsAsync(email);
        }
        public Task<User> GetAsync(Guid id, bool includeRole = false)
        {
            var convertParams = includeRole ? new UserConvertParams { IncludeRole = true } : null;
            return _userDal.GetAsync(id, convertParams);
        }
        public Task<bool> DeleteAsync(Guid id)
        {
            return _userDal.DeleteAsync(id);
        }
        public Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, bool includeRole = false)
        {
            var convertParams = includeRole ? new UserConvertParams { IncludeRole = true } : null;
            return _userDal.GetAsync(searchParams, convertParams);
        }
        public async Task<User?> VerifyPasswordAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var searchParams = new UserSearchParams { Email = email };
            var dalResult = await _userDal.GetAsync(searchParams);
            var user = dalResult.Objects.FirstOrDefault();

            if (user != null && PasswordHasher.VerifyPassword(user.PasswordHash, password))
            {
                return user;
            }

            return null;
        }
        /// <summary>
        /// Проверяет сложность пароля согласно настройкам
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        private void ValidatePassword(string password)
        {
            if (password.Length < _options.MinPasswordLength)
            {
                throw new ArgumentException(
                    $"Пароль должен содержать не менее {_options.MinPasswordLength} символов",
                    nameof(password));
            }

            if (_options.RequireComplexPassword)
            {
                bool hasDigit = password.Any(char.IsDigit);
                bool hasLetter = password.Any(char.IsLetter);
                bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

                if (!hasDigit || !hasLetter || !hasSpecial)
                {
                    throw new ArgumentException(
                        "Пароль должен содержать буквы, цифры и специальные символы",
                        nameof(password));
                }
            }
        }
        /// <summary>
        /// Проверяет валидность формата email
        /// </summary>
        private bool IsValidEmail(string email)
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
    }
}
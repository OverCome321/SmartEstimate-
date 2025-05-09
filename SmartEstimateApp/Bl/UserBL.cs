using Bl.DI;
using Bl.Interfaces;
using Bl.Managers;
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
        /// <param name="userDal">Слой доступа к данным для пользователей</param>
        /// <param name="options">Настройки бизнес-логики (опционально)</param>
        public UserBL(IUserDal userDal, IOptions<BusinessLogicOptions> options = null)
        {
            _userDal = userDal ?? throw new ArgumentNullException(nameof(userDal));
            _options = options?.Value ?? new BusinessLogicOptions();
        }

        /// <summary>
        /// Добавляет или обновляет сущность пользователя
        /// </summary>
        /// <param name="entity">Сущность пользователя для добавления или обновления</param>
        /// <returns>Идентификатор добавленного или обновленного пользователя</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если email некорректен или роль не указана</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если email уже существует</exception>
        public async Task<long> AddOrUpdateAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.PasswordHash = PasswordHasher.HashPassword(entity.PasswordHash);

            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
            }

            return await _userDal.AddOrUpdateAsync(entity);
        }

        public async Task ValidationCommand(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!Validation.IsValidEmail(entity.Email))
                throw new ArgumentException(ErrorMessages.EmailInvalidFormat, nameof(entity.Email));

            if (entity.Role == null || entity.Role.Id == 0)
                throw new ArgumentException(ErrorMessages.RoleNotSpecified, nameof(entity.Role));

            if (_options.EnableExtendedValidation)
                Validation.ValidatePassword(entity.PasswordHash, _options);

            if (await _userDal.ExistsAsync(entity.Email))
                throw new InvalidOperationException(ErrorMessages.EmailAlreadyExists);
        }

        /// <summary>
        /// Проверяет существование пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя для проверки</param>
        /// <returns>True, если пользователь существует, иначе false</returns>
        public Task<bool> ExistsAsync(long id) => _userDal.ExistsAsync(id);

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True, если пользователь существует, иначе false</returns>
        public Task<bool> ExistsAsync(string email) => _userDal.ExistsAsync(email);

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="includeRole">Включать ли информацию о роли</param>
        /// <returns>Сущность пользователя</returns>
        public Task<User> GetAsync(long id, bool includeRole = false) => _userDal.GetAsync(id, includeRole);

        public Task<User> GetAsync(string email, bool includeRole = false) => _userDal.GetAsync(email, includeRole);

        /// <summary>
        /// Удаляет пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя для удаления</param>
        /// <returns>True, если удаление успешно, иначе false</returns>
        public Task<bool> DeleteAsync(long id) => _userDal.DeleteAsync(id);


        /// <summary>
        /// Получает пользователей на основе параметров поиска пользователей
        /// </summary>
        /// <param name="searchParams">Параметры поиска пользователей</param>
        /// <param name="includeRole">Включать ли информацию о роли</param>
        /// <returns>Результат поиска с пользователями</returns>
        public Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, bool includeRole = true)
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }
            return _userDal.GetAsync(searchParams, includeRole);
        }

        /// <summary>
        /// Проверяет учетные данные пользователя
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Сущность пользователя, если данные верны, иначе null</returns>
        public async Task<User?> VerifyPasswordAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var searchParams = new UserSearchParams(email);

            var dalResult = await _userDal.GetAsync(searchParams);

            var user = dalResult.Objects.FirstOrDefault();

            if (user != null && PasswordHasher.VerifyPassword(user.PasswordHash, password))
            {
                return user;
            }

            return null;
        }

    }
}
using Bl.DI;
using Bl.Interfaces;
using Bl.Managers;
using Common.Search;
using Common.Security;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserBL> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserBL"/>
        /// </summary>
        /// <param name="userDal">Слой доступа к данным для пользователей</param>
        /// <param name="options">Настройки бизнес-логики (опционально)</param>
        /// <param name="logger">Логгер</param>
        public UserBL(IUserDal userDal, IOptions<BusinessLogicOptions> options = null, ILogger<UserBL> logger = null)
        {
            _userDal = userDal ?? throw new ArgumentNullException(nameof(userDal));
            _options = options?.Value ?? new BusinessLogicOptions();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Попытка добавить или обновить пользователя с null entity");
                    throw new ArgumentNullException(nameof(entity));
                }

                entity.PasswordHash = PasswordHasher.HashPassword(entity.PasswordHash);

                if (entity.Id == 0)
                {
                    entity.CreatedAt = DateTime.Now;
                }

                var id = await _userDal.AddOrUpdateAsync(entity);
                _logger.LogInformation("Пользователь успешно добавлен/обновлен: {@User}", entity);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении или обновлении пользователя: {@User}", entity);
                throw;
            }
        }

        /// <summary>
        /// Метод для валидации
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> Пустое значение</exception>
        /// <exception cref="ArgumentException"> Невалидный email или не указана роль при валидации</exception>
        /// <exception cref="InvalidOperationException"> Попытка добавить пользователя с уже существующим email</exception>
        public async Task ValidationCommand(User entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Попытка валидации пользователя с null entity");
                    throw new ArgumentNullException(nameof(entity));
                }

                if (!Validation.IsValidEmail(entity.Email))
                {
                    _logger.LogWarning("Невалидный email при валидации: {Email}", entity.Email);
                    throw new ArgumentException(ErrorMessages.EmailInvalidFormat, nameof(entity.Email));
                }

                if (entity.Role == null || entity.Role.Id == 0)
                {
                    _logger.LogWarning("Не указана роль при валидации пользователя: {@User}", entity);
                    throw new ArgumentException(ErrorMessages.RoleNotSpecified, nameof(entity.Role));
                }

                if (_options.EnableExtendedValidation)
                {
                    Validation.ValidatePassword(entity.PasswordHash, _options);
                }

                if (await _userDal.ExistsAsync(entity.Email))
                {
                    _logger.LogWarning("Попытка добавить пользователя с уже существующим email: {Email}", entity.Email);
                    throw new InvalidOperationException(ErrorMessages.EmailAlreadyExists);
                }

                _logger.LogDebug("Валидация пользователя прошла успешно: {@User}", entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка валидации пользователя: {@User}", entity);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя для проверки</param>
        /// <returns>True, если пользователь существует, иначе false</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            try
            {
                var exists = await _userDal.ExistsAsync(id);
                _logger.LogDebug("Проверка существования пользователя по Id={Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True, если пользователь существует, иначе false</returns>
        public async Task<bool> ExistsAsync(string email)
        {
            try
            {
                var exists = await _userDal.ExistsAsync(email);
                _logger.LogDebug("Проверка существования пользователя по Email={Email}: {Exists}", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Email={Email})", email);
                throw;
            }
        }

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="includeRole">Включать ли информацию о роли</param>
        /// <returns>Сущность пользователя</returns>
        public async Task<User> GetAsync(long id, bool includeRole = false)
        {
            try
            {
                var user = await _userDal.GetAsync(id, includeRole);
                if (user == null)
                    _logger.LogInformation("Пользователь не найден по Id={Id}", id);
                else
                    _logger.LogDebug("Получен пользователь: {@User}", user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Метод для получения пользователя по логину
        /// </summary>
        /// <param name="email"> Логин </param>
        /// <param name="includeRole"> Получать ли роль</param>
        /// <returns></returns>
        public async Task<User> GetAsync(string email, bool includeRole = false)
        {
            try
            {
                var user = await _userDal.GetAsync(email, includeRole);
                if (user == null)
                    _logger.LogInformation("Пользователь не найден по Email={Email}", email);
                else
                    _logger.LogDebug("Получен пользователь: {@User}", user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя Email={Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Удаляет пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя для удаления</param>
        /// <returns>True, если удаление успешно, иначе false</returns>
        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var deleted = await _userDal.DeleteAsync(id);
                _logger.LogInformation("Удаление пользователя Id={Id}: результат={Deleted}", id, deleted);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Получает пользователей на основе параметров поиска пользователей
        /// </summary>
        /// <param name="searchParams">Параметры поиска пользователей</param>
        /// <param name="includeRole">Включать ли информацию о роли</param>
        /// <returns>Результат поиска с пользователями</returns>
        public async Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, bool includeRole = true)
        {
            try
            {
                if (searchParams == null)
                {
                    _logger.LogWarning("Попытка поиска пользователей с null параметрами поиска");
                    throw new ArgumentNullException(nameof(searchParams));
                }

                var result = await _userDal.GetAsync(searchParams, includeRole);

                _logger.LogDebug("Результат поиска пользователей по параметрам {@SearchParams}: {@Result}", searchParams, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске пользователей по параметрам {@SearchParams}", searchParams);
                throw;
            }
        }

        /// <summary>
        /// Проверяет учетные данные пользователя
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Сущность пользователя, если данные верны, иначе null</returns>
        public async Task<User?> VerifyPasswordAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Попытка проверки пароля с пустыми email или password");
                    return null;
                }

                if (!Validation.IsValidEmail(email))
                {
                    _logger.LogWarning("Невалидный email при проверке пароля: {Email}", email);
                    throw new ArgumentException(ErrorMessages.EmailInvalidFormat, nameof(email));
                }

                var searchParams = new UserSearchParams(email);

                var dalResult = await _userDal.GetAsync(searchParams);

                var user = dalResult.Objects.FirstOrDefault();

                if (user != null && PasswordHasher.VerifyPassword(user.PasswordHash, password))
                {
                    _logger.LogInformation("Проверка пароля успешна для пользователя: {Email}", email);
                    return user;
                }

                _logger.LogWarning("Проверка пароля неудачна для пользователя: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке пароля пользователя: {Email}", email);
                throw;
            }
        }
    }
}
using Bl.Interfaces;
using Bl.Managers;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;

namespace Bl
{
    /// <summary>
    /// Бизнес-логика для работы с клиентами
    /// </summary>
    public class ClientBL : IClientBL
    {
        private readonly IClientDal _clientDal;
        private readonly ILogger<ClientBL> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClientBL"/>
        /// </summary>
        /// <param name="clientDal">Слой доступа к данным для клиентов</param>
        /// <param name="logger">Логгер</param>
        public ClientBL(IClientDal clientDal, ILogger<ClientBL> logger)
        {
            _clientDal = clientDal ?? throw new ArgumentNullException(nameof(clientDal));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Добавляет или обновляет сущность клиента
        /// </summary>
        /// <param name="entity">Сущность клиента для добавления или обновления</param>
        /// <returns>Идентификатор добавленного или обновленного клиента</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если email, телефон или идентификатор пользователя некорректны</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если email или телефон уже существуют</exception>
        public async Task<long> AddOrUpdateAsync(Client entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Попытка добавить или обновить клиента с null entity");
                    throw new ArgumentNullException(nameof(entity), ErrorMessages.ClientEntityNull);
                }

                if (!string.IsNullOrWhiteSpace(entity.Email) && !Validation.IsValidEmail(entity.Email))
                {
                    _logger.LogWarning("Попытка добавить/обновить клиента с некорректным email: {Email}", entity.Email);
                    throw new ArgumentException(ErrorMessages.InvalidEmailFormat, nameof(entity.Email));
                }

                if (!string.IsNullOrWhiteSpace(entity.Phone) && !Validation.IsValidPhone(entity.Phone))
                {
                    _logger.LogWarning("Попытка добавить/обновить клиента с некорректным телефоном: {Phone}", entity.Phone);
                    throw new ArgumentException(ErrorMessages.InvalidPhoneFormat, nameof(entity.Phone));
                }

                if (entity.Id == 0 && await _clientDal.ExistsAsync(entity.Email, entity.User.Id))
                {
                    _logger.LogWarning("Попытка добавить клиента с уже существующим email: {Email}, userId: {UserId}", entity.Email, entity.User.Id);
                    throw new InvalidOperationException(ErrorMessages.ClientEmailAlreadyExists);
                }

                if (entity.Id == 0 && await _clientDal.ExistsPhoneAsync(entity.Phone, entity.User.Id))
                {
                    _logger.LogWarning("Попытка добавить клиента с уже существующим телефоном: {Phone}, userId: {UserId}", entity.Phone, entity.User.Id);
                    throw new InvalidOperationException(ErrorMessages.ClientPhoneAlreadyExists);
                }

                if (entity.Id == 0)
                {
                    entity.CreatedAt = DateTime.Now;
                }

                var result = await _clientDal.AddOrUpdateAsync(entity);

                _logger.LogInformation("Клиент успешно добавлен/обновлен: {@Client}", entity);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении или обновлении клиента: {@Client}", entity);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            try
            {
                var exists = await _clientDal.ExistsAsync(id);
                _logger.LogDebug("Проверка существования клиента по Id={Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование клиента по email и идентификатору пользователя
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public async Task<bool> ExistsAsync(string email, long userId)
        {
            try
            {
                var exists = await _clientDal.ExistsAsync(email, userId);
                _logger.LogDebug("Проверка существования клиента по Email={Email}, UserId={UserId}: {Exists}", email, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Email={Email}, UserId={UserId})", email, userId);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование клиента по телефону и идентификатору пользователя
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public async Task<bool> ExistsPhoneAsync(string phone, long userId)
        {
            try
            {
                var exists = await _clientDal.ExistsPhoneAsync(phone, userId);
                _logger.LogDebug("Проверка существования клиента по Phone={Phone}, UserId={UserId}: {Exists}", phone, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsPhoneAsync(Phone={Phone}, UserId={UserId})", phone, userId);
                throw;
            }
        }

        /// <summary>
        /// Получает клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="includeRelated">Включать ли связанные данные</param>
        /// <returns>Сущность клиента</returns>
        public async Task<Client> GetAsync(long id, bool includeRelated = false)
        {
            try
            {
                var client = await _clientDal.GetAsync(id, includeRelated);
                if (client == null)
                    _logger.LogInformation("Клиент не найден по Id={Id}", id);
                else
                    _logger.LogDebug("Получен клиент: {@Client}", client);
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Удаляет клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если удаление успешно; иначе false</returns>
        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var deleted = await _clientDal.DeleteAsync(id);
                _logger.LogInformation("Удаление клиента Id={Id}: результат={Deleted}", id, deleted);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Получает список клиентов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с клиентами</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если параметры поиска null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если идентификатор пользователя не указан</exception>
        public async Task<SearchResult<Client>> GetAsync(ClientSearchParams searchParams, bool includeRelated = true)
        {
            try
            {
                if (searchParams == null)
                {
                    _logger.LogWarning("Попытка поиска клиентов с null параметрами поиска");
                    throw new ArgumentNullException(nameof(searchParams));
                }

                if (!searchParams.UserId.HasValue)
                {
                    _logger.LogWarning("Попытка поиска клиентов с неуказанным идентификатором пользователя");
                    throw new ArgumentException(ErrorMessages.UserIdRequired, nameof(searchParams.UserId));
                }

                var result = await _clientDal.GetAsync(searchParams, includeRelated);

                _logger.LogDebug("Результат поиска клиентов по параметрам {@SearchParams}: {@Result}", searchParams, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске клиентов по параметрам {@SearchParams}", searchParams);
                throw;
            }
        }
    }
}
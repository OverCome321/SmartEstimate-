using Bl.Interfaces;
using Bl.Managers;
using Common.Search;
using Dal.Interfaces;
using Entities;

namespace Bl
{
    /// <summary>
    /// Бизнес-логика для работы с клиентами
    /// </summary>
    public class ClientBL : IClientBL
    {
        private readonly IClientDal _clientDal;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClientBL"/>
        /// </summary>
        /// <param name="clientDal">Слой доступа к данным для клиентов</param>
        public ClientBL(IClientDal clientDal)
        {
            _clientDal = clientDal ?? throw new ArgumentNullException(nameof(clientDal));
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
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), ErrorMessages.ClientEntityNull);

            if (!string.IsNullOrWhiteSpace(entity.Email) && !Validation.IsValidEmail(entity.Email))
                throw new ArgumentException(ErrorMessages.InvalidEmailFormat, nameof(entity.Email));

            if (!string.IsNullOrWhiteSpace(entity.Phone) && !Validation.IsValidPhone(entity.Phone))
                throw new ArgumentException(ErrorMessages.InvalidPhoneFormat, nameof(entity.Phone));

            if (entity.Id == 0)
                throw new ArgumentException(ErrorMessages.UserIdNotSpecified, nameof(entity.Id));

            if (await _clientDal.ExistsAsync(entity.Email, entity.User.Id))
                throw new InvalidOperationException(ErrorMessages.ClientEmailAlreadyExists);

            if (await _clientDal.ExistsPhoneAsync(entity.Phone, entity.User.Id))
                throw new InvalidOperationException(ErrorMessages.ClientPhoneAlreadyExists);

            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
            }

            return await _clientDal.AddOrUpdateAsync(entity);
        }

        /// <summary>
        /// Проверяет существование клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public Task<bool> ExistsAsync(long id) => _clientDal.ExistsAsync(id);

        /// <summary>
        /// Проверяет существование клиента по email и идентификатору пользователя
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public Task<bool> ExistsAsync(string email, long userId) => _clientDal.ExistsAsync(email, userId);

        /// <summary>
        /// Проверяет существование клиента по телефону и идентификатору пользователя
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        public Task<bool> ExistsPhoneAsync(string phone, long userId) => _clientDal.ExistsPhoneAsync(phone, userId);

        /// <summary>
        /// Получает клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="includeRelated">Включать ли связанные данные</param>
        /// <returns>Сущность клиента</returns>
        public Task<Client> GetAsync(long id, bool includeRelated = false) => _clientDal.GetAsync(id, null);

        /// <summary>
        /// Удаляет клиента по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если удаление успешно; иначе false</returns>
        public Task<bool> DeleteAsync(long id) => _clientDal.DeleteAsync(id);

        /// <summary>
        /// Получает список клиентов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с клиентами</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если параметры поиска null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если идентификатор пользователя не указан</exception>
        public Task<SearchResult<Client>> GetAsync(ClientSearchParams searchParams)
        {
            if (searchParams == null)
                throw new ArgumentNullException(nameof(searchParams));

            if (!searchParams.UserId.HasValue)
                throw new ArgumentException(ErrorMessages.UserIdRequired, nameof(searchParams.UserId));

            return _clientDal.GetAsync(searchParams, null);
        }
    }
}
using Common.Search;
using Entities;

namespace Bl.Interfaces
{
    /// <summary>
    /// Интерфейс бизнес-логики для работы с клиентами
    /// </summary>
    public interface IClientBL : IBaseBL<Client, long>
    {
        /// <summary>
        /// Проверяет существование клиента по email и идентификатору пользователя
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        Task<bool> ExistsAsync(string email, long userId);

        /// <summary>
        /// Проверяет существование клиента по телефону и идентификатору пользователя
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует; иначе false</returns>
        Task<bool> ExistsPhoneAsync(string phone, long userId);

        /// <summary>
        /// Получает список клиентов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с клиентами</returns>
        Task<SearchResult<Client>> GetAsync(ClientSearchParams searchParams, bool includeRelated = true);
    }
}
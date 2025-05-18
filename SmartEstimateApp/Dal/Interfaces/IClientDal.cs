using Common.Search;
using Entities;

namespace Dal.Interfaces
{
    /// <summary>
    /// Интерфейс доступа к данным клиентов
    /// </summary>
    public interface IClientDal : IBaseDal<Client, long, ClientSearchParams, object>
    {
        /// <summary>
        /// Проверяет существование клиента по ID 
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если клиент существует для указанного пользователя, иначе False</returns>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Проверяет существование клиента по email и UserId
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким email существует для указанного пользователя, иначе False</returns>
        Task<bool> ExistsAsync(string email, long userId);

        /// <summary>
        /// Проверяет существование клиента по телефону и UserId
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким телефоном существует для указанного пользователя, иначе False</returns>
        Task<bool> ExistsPhoneAsync(string phone, long userId);
    }
}
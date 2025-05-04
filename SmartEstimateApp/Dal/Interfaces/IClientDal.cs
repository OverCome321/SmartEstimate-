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
        /// <returns>True, если клиент существует, иначе False</returns>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Проверяет существование клиента по email
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <returns>True, если клиент с таким email существует, иначе False</returns>
        Task<bool> ExistsAsync(string email);
        /// <summary>
        /// Проверяет существование клиента по phone
        /// </summary>
        /// <param name="phone">Phone клиента</param>
        /// <returns>True, если клиент с таким phone существует, иначе False</returns>
        Task<bool> ExistsPhoneAsync(string phone);
    }
}
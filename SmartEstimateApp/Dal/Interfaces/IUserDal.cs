using Common.Convert;
using Common.Search;
using Entities;

namespace Dal.Interfaces
{
    /// <summary>
    /// Интерфейс доступа к данным пользователей
    /// </summary>
    public interface IUserDal : IBaseDal<User, long, UserSearchParams, ConvertParams>
    {
        /// <summary>
        /// Проверяет существование пользователя по ID
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>True, если пользователь существует, иначе False</returns>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>True, если пользователь с таким email существует, иначе False</returns>
        Task<bool> ExistsAsync(string email);

        /// <summary>
        /// Проверяет существование роли по ID
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <returns>True, если роль существует, иначе False</returns>
        Task<bool> RoleExistsAsync(long roleId);

        /// <summary>
        /// Получает пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="isFull">Флаг полной загрузки (включая связанные данные, например, роль)</param>
        /// <returns>Найденная сущность или null</returns>
        Task<User> GetAsync(string email, bool isFull = true);
    }
}
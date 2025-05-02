using Common.Search;
using Entities;

namespace Bl.Interfaces
{
    /// <summary>
    /// Интерфейс бизнес-логики для работы с пользователями
    /// </summary>
    public interface IUserBL
    {
        /// <summary>
        /// Добавляет или обновляет пользователя
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <returns>Идентификатор добавленного или обновленного пользователя</returns>
        Task<Guid> AddOrUpdateAsync(User entity);

        /// <summary>
        /// Проверяет существование пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>True, если пользователь существует; иначе false</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>True, если пользователь существует; иначе false</returns>
        Task<bool> ExistsAsync(string email);

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="includeRole">Включать ли данные о роли пользователя</param>
        /// <returns>Найденный пользователь или null</returns>
        Task<User> GetAsync(Guid id, bool includeRole = false);

        /// <summary>
        /// Удаляет пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>True, если пользователь был удален; иначе false</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Получает список пользователей по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <param name="includeRole">Включать ли данные о роли пользователя</param>
        /// <returns>Результат поиска с пользователями</returns>
        Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, bool includeRole = false);

        /// <summary>
        /// Проверяет пароль пользователя
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль для проверки</param>
        /// <returns>Пользователь, если пароль верный; иначе null</returns>
        Task<User?> VerifyPasswordAsync(string email, string password);
    }
}
using Common.Search;
using Entities;

namespace Bl.Interfaces
{
    /// <summary>
    /// Интерфейс бизнес-логики для работы с пользователями
    /// </summary>
    public interface IUserBL : IBaseBL<User, long>
    {
        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>True, если пользователь существует; иначе false</returns>
        Task<bool> ExistsAsync(string email);

        /// <summary>
        /// Проверяет пароль пользователя
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль для проверки</param>
        /// <returns>Пользователь, если пароль верный; иначе null</returns>
        Task<User?> VerifyPasswordAsync(string email, string password);

        /// <summary>
        /// Получает список сущностей по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <param name="includeRole">Включать ли связанные роль</param>
        /// <returns>Результат поиска с сущностями</returns>
        Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, bool includeRole = false);
    }
}
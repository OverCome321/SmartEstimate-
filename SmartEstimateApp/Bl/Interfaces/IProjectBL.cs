using Common.Search;
using Entities;

namespace Bl.Interfaces
{
    /// <summary>
    /// Интерфейс бизнес-логики для работы с проектами
    /// </summary>
    public interface IProjectBL : IBaseBL<Project, long>
    {
        /// <summary>
        /// Проверяет существование проекта по имени, идентификатору клиента и идентификатору пользователя
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если проект существует и доступен пользователю; иначе false</returns>
        Task<bool> ExistsAsync(string name, long clientId, long userId);

        /// <summary>
        /// Получает список проектов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с проектами</returns>
        Task<SearchResult<Project>> GetAsync(ProjectSearchParams searchParams, bool includeRelated = true);
    }
}
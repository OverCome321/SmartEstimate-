using Common.Convert;
using Common.Search;
using Entities;

namespace Dal.Interfaces
{
    /// <summary>
    /// Интерфейс доступа к данным проектов
    /// </summary>
    public interface IProjectDal : IBaseDal<Project, long, ProjectSearchParams, ConvertParams>
    {
        /// <summary>
        /// Проверяет существование проекта по ID
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если проект существует, иначе False</returns>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Проверяет существование проекта по имени, клиенту и UserId
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если проект с таким именем существует для указанного клиента и пользователя, иначе False</returns>
        Task<bool> ExistsAsync(string name, long clientId, long userId);
    }
}
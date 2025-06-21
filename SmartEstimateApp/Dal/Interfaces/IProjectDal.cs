using Common.Convert;
using Common.Search;
using Entities;

namespace Dal.Interfaces
{
    public interface IProjectDal : IBaseDal<Project, long, ProjectSearchParams, ConvertParams>
    {
        /// <summary>
        /// Проверяет существование проекта по ID.
        /// </summary>
        /// <param name="id">Идентификатор проекта.</param>
        /// <returns>True, если проект существует, иначе False.</returns>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Проверяет существование проекта по имени, клиенту и проверяет доступ пользователя.
        /// </summary>
        /// <param name="name">Название проекта.</param>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <param name="userId">Идентификатор пользователя для проверки доступа.</param>
        /// <returns>True, если проект с таким именем существует и доступен пользователю, иначе False.</returns>
        Task<bool> ExistsAsync(string name, long clientId, long userId);

        /// <summary>
        /// Добавляет в базу данных новый проект вместе со всеми дочерними сущностями (сметами и их позициями).
        /// </summary>
        /// <param name="projectEntity">Сущность нового проекта для добавления.</param>
        /// <returns>Идентификатор созданного проекта.</returns>
        Task<Dal.DbModels.Project> AddNewProjectGraphAsync(Project projectEntity);

        /// <summary>
        /// Обновляет существующий проект в базе данных, синхронизируя все его дочерние коллекции
        /// (удаляя, обновляя и добавляя сметы и их позиции).
        /// </summary>
        /// <param name="projectEntity">Сущность проекта с обновленными данными.</param>
        /// <param name="existingProject">Уже загруженная из БД и отслеживаемая EF Core сущность проекта.</param>
        /// <returns>Идентификатор обновленного проекта.</returns>
        Task<Dal.DbModels.Project> UpdateExistingProjectGraphAsync(Project projectEntity, Dal.DbModels.Project existingProject);
    }
}
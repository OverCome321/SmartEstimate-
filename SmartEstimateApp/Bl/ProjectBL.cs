using Bl.Interfaces;
using Common.Search;
using Dal.Interfaces;
using Entities;

namespace Bl
{
    /// <summary>
    /// Бизнес-логика для работы с проектами
    /// </summary>
    public class ProjectBL : IProjectBL
    {
        private readonly IProjectDal _projectDal;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProjectBL"/>
        /// </summary>
        /// <param name="projectDal">Слой доступа к данным для проектов</param>
        public ProjectBL(IProjectDal projectDal)
        {
            _projectDal = projectDal ?? throw new ArgumentNullException(nameof(projectDal));
        }

        /// <summary>
        /// Добавляет или обновляет сущность проекта
        /// </summary>
        /// <param name="entity">Сущность проекта для добавления или обновления</param>
        /// <returns>Идентификатор добавленного или обновленного проекта</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если сущность null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если название проекта или идентификатор клиента некорректны</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если проект с таким названием уже существует у клиента</exception>
        public async Task<long> AddOrUpdateAsync(Project entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException(ErrorMessages.ProjectNameRequired, nameof(entity.Name));

            if (entity.Client == null)
                throw new ArgumentException(ErrorMessages.ClientIdNotSpecified, nameof(entity.Client));

            if (entity.Id == 0 && await _projectDal.ExistsAsync(entity.Id))
                throw new InvalidOperationException(ErrorMessages.ProjectNameAlreadyExists);

            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
            }
            entity.UpdatedAt = DateTime.Now;

            return await _projectDal.AddOrUpdateAsync(entity);
        }

        /// <summary>
        /// Проверяет существование проекта по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если проект существует; иначе false</returns>
        public Task<bool> ExistsAsync(long id) => _projectDal.ExistsAsync(id);

        /// <summary>
        /// Проверяет существование проекта по имени, идентификатору клиента и идентификатору пользователя
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если проект существует и доступен пользователю; иначе false</returns>
        public Task<bool> ExistsAsync(string name, long clientId, long userId) => _projectDal.ExistsAsync(name, clientId, userId);

        /// <summary>
        /// Получает проект по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <param name="includeRelated">Включать ли связанные данные</param>
        /// <returns>Сущность проекта</returns>
        public Task<Project> GetAsync(long id, bool includeRelated = false) => _projectDal.GetAsync(id, includeRelated);

        /// <summary>
        /// Удаляет проект по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если удаление успешно; иначе false</returns>
        public Task<bool> DeleteAsync(long id) => _projectDal.DeleteAsync(id);

        /// <summary>
        /// Получает список проектов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с проектами</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если параметры поиска null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если идентификатор пользователя не указан</exception>
        public Task<SearchResult<Project>> GetAsync(ProjectSearchParams searchParams, bool includeRelated = true)
        {
            if (searchParams == null)
                throw new ArgumentNullException(nameof(searchParams));

            if (!searchParams.UserId.HasValue)
                throw new ArgumentException(ErrorMessages.UserIdRequired, nameof(searchParams.UserId));

            return _projectDal.GetAsync(searchParams, includeRelated);
        }
    }
}
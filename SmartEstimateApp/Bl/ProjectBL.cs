using Bl.Interfaces;
using Common.Search;
using Dal.Interfaces;
using Entities;
using Microsoft.Extensions.Logging;

namespace Bl
{
    /// <summary>
    /// Бизнес-логика для работы с проектами
    /// </summary>
    public class ProjectBL : IProjectBL
    {
        private readonly IProjectDal _projectDal;
        private readonly ILogger<ProjectBL> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProjectBL"/>
        /// </summary>
        /// <param name="projectDal">Слой доступа к данным для проектов</param>
        /// <param name="logger">Логгер</param>
        public ProjectBL(IProjectDal projectDal, ILogger<ProjectBL> logger)
        {
            _projectDal = projectDal ?? throw new ArgumentNullException(nameof(projectDal));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Попытка добавить или обновить проект с null entity");
                    throw new ArgumentNullException(nameof(entity));
                }

                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    _logger.LogWarning("Попытка добавить/обновить проект с пустым именем");
                    throw new ArgumentException(ErrorMessages.ProjectNameRequired, nameof(entity.Name));
                }

                if (entity.Client == null)
                {
                    _logger.LogWarning("Попытка добавить/обновить проект с неуказанным клиентом");
                    throw new ArgumentException(ErrorMessages.ClientIdNotSpecified, nameof(entity.Client));
                }

                if (entity.Id == 0 && await _projectDal.ExistsAsync(entity.Id))
                {
                    _logger.LogWarning("Попытка добавить проект с уже существующим именем: {Name}", entity.Name);
                    throw new InvalidOperationException(ErrorMessages.ProjectNameAlreadyExists);
                }

                if (entity.Id == 0)
                {
                    entity.CreatedAt = DateTime.Now;
                }
                entity.UpdatedAt = DateTime.Now;

                var result = await _projectDal.AddOrUpdateAsync(entity);

                _logger.LogInformation("Проект успешно добавлен/обновлен: {@Project}", entity);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении или обновлении проекта: {@Project}", entity);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование проекта по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если проект существует; иначе false</returns>
        public Task<bool> ExistsAsync(long id)
        {
            try
            {
                var exists = _projectDal.ExistsAsync(id);
                _logger.LogDebug("Проверка существования проекта по Id={Id}: {Exists}", id, exists.Result);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование проекта по имени, идентификатору клиента и идентификатору пользователя
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если проект существует и доступен пользователю; иначе false</returns>
        public Task<bool> ExistsAsync(string name, long clientId, long userId)
        {
            try
            {
                var exists = _projectDal.ExistsAsync(name, clientId, userId);
                _logger.LogDebug("Проверка существования проекта по Name={Name}, ClientId={ClientId}, UserId={UserId}: {Exists}", name, clientId, userId, exists.Result);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Name={Name}, ClientId={ClientId}, UserId={UserId})", name, clientId, userId);
                throw;
            }
        }

        /// <summary>
        /// Получает проект по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <param name="includeRelated">Включать ли связанные данные</param>
        /// <returns>Сущность проекта</returns>
        public Task<Project> GetAsync(long id, bool includeRelated = false)
        {
            try
            {
                var project = _projectDal.GetAsync(id, includeRelated);
                _logger.LogDebug("Получен проект по Id={Id}: {@Project}", id, project.Result);
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении проекта Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Удаляет проект по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если удаление успешно; иначе false</returns>
        public Task<bool> DeleteAsync(long id)
        {
            try
            {
                var deleted = _projectDal.DeleteAsync(id);
                _logger.LogInformation("Удаление проекта Id={Id}: результат={Deleted}", id, deleted.Result);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении проекта Id={Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Получает список проектов по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Результат поиска с проектами</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если параметры поиска null</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если идентификатор пользователя не указан</exception>
        public Task<SearchResult<Project>> GetAsync(ProjectSearchParams searchParams, bool includeRelated = true)
        {
            try
            {
                if (searchParams == null)
                {
                    _logger.LogWarning("Попытка поиска проектов с null параметрами поиска");
                    throw new ArgumentNullException(nameof(searchParams));
                }

                if (!searchParams.UserId.HasValue)
                {
                    _logger.LogWarning("Попытка поиска проектов с неуказанным идентификатором пользователя");
                    throw new ArgumentException(ErrorMessages.UserIdRequired, nameof(searchParams.UserId));
                }

                var result = _projectDal.GetAsync(searchParams, includeRelated);
                _logger.LogDebug("Результат поиска проектов по параметрам {@SearchParams}: {@Result}", searchParams, result.Result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске проектов по параметрам {@SearchParams}", searchParams);
                throw;
            }
        }
    }
}
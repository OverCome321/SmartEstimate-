using AutoMapper;
using Common.Convert;
using Common.Search;
using Dal.DbModels;
using Dal.Helpers;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dal.Layers
{
    /// <summary>
    /// Класс доступа к данным проектов
    /// </summary>
    public class ProjectDal : BaseDal<Dal.DbModels.Project, Entities.Project, long, ProjectSearchParams, ConvertParams>, IProjectDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly ILogger<ProjectDal> _logger;

        /// <summary>
        /// Конструктор класса ProjectDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования между моделями</param> 
        /// /// <param name="logger">Логгер</param>
        public ProjectDal(SmartEstimateDbContext context, IMapper mapper, ILogger<ProjectDal> logger) : base(mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Определяет, требуются ли дополнительные обновления после сохранения объекта
        /// </summary>
        protected override bool RequiresUpdatesAfterObjectSaving => true;

        /// <summary>
        /// Проверяет существование проекта по ID
        /// </summary>
        /// <param name="id">Идентификатор проекта</param>
        /// <returns>True, если проект существует, иначе False</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            try
            {
                var exists = await _context.Projects.AnyAsync(p => p.Id == id);
                _logger.LogDebug("Проверка существования проекта по Id={Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование проекта по имени и клиенту
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>True, если проект с таким именем существует для указанного клиента, иначе False</returns>
        public async Task<bool> ExistsAsync(string name, long clientId)
        {
            try
            {
                var exists = await _context.Projects.AnyAsync(p => p.Name == name && p.ClientId == clientId);
                _logger.LogDebug("Проверка существования проекта по Name={Name}, ClientId={ClientId}: {Exists}", name, clientId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Name={Name}, ClientId={ClientId})", name, clientId);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование проекта по имени, клиенту и проверяет доступ пользователя
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя для проверки доступа</param>
        /// <returns>True, если проект с таким именем существует и доступен пользователю, иначе False</returns>
        public async Task<bool> ExistsAsync(string name, long clientId, long userId)
        {
            try
            {
                var exists = await _context.Projects
                    .Include(p => p.Client)
                    .AnyAsync(p => p.Name == name && p.ClientId == clientId && p.Client.UserId == userId);
                _logger.LogDebug("Проверка существования проекта по Name={Name}, ClientId={ClientId}, UserId={UserId}: {Exists}", name, clientId, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Name={Name}, ClientId={ClientId}, UserId={UserId})", name, clientId, userId);
                throw;
            }
        }

        /// <summary>
        /// Основной метод добавления или обновления проекта.
        /// Определяет, нужно ли создавать новую запись или обновлять существующую,
        /// и делегирует выполнение соответствующим приватным методам.
        /// </summary>
        protected override async Task<Dal.DbModels.Project> AddOrUpdateInternalAsync(Entities.Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var existingProject = entity.Id > 0
                    ? await _context.Projects
                        .Include(p => p.Estimates)
                            .ThenInclude(e => e.Items)
                        .FirstOrDefaultAsync(p => p.Id == entity.Id)
                    : null;

                if (existingProject != null)
                {
                    _logger.LogInformation("Обновление проекта Id={ProjectId}", entity.Id);
                    return await UpdateExistingProjectGraphAsync(entity, existingProject);
                }
                else
                {
                    _logger.LogInformation("Добавление нового проекта: {ProjectName}", entity.Name);
                    return await AddNewProjectGraphAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении проекта: {@ProjectEntity}", entity);
                throw;
            }
        }

        /// <summary>
        /// Добавляет в базу данных новый проект вместе со всеми дочерними сущностями.
        /// </summary>
        public async Task<Dal.DbModels.Project> AddNewProjectGraphAsync(Entities.Project projectEntity)
        {
            var newDbProject = _mapper.Map<Dal.DbModels.Project>(projectEntity);

            await UpdateBeforeSavingAsync(projectEntity, newDbProject, false);

            newDbProject.Estimates = _mapper.Map<List<Dal.DbModels.Estimate>>(projectEntity.Estimates);

            await _context.Projects.AddAsync(newDbProject);

            return newDbProject;
        }

        /// <summary>
        /// Обновляет существующий проект, синхронизируя его дочерние коллекции.
        /// </summary>
        public async Task<Dal.DbModels.Project> UpdateExistingProjectGraphAsync(Entities.Project projectEntity, Dal.DbModels.Project existingProject)
        {
            _mapper.Map(projectEntity, existingProject);
            await UpdateBeforeSavingAsync(projectEntity, existingProject, true);

            CollectionSyncHelper.Sync(
                dbCollection: existingProject.Estimates,
                dtoCollection: projectEntity.Estimates,
                keySelector: estimate => estimate.Id,
                updateAction: (dbEstimate, dtoEstimate) =>
                {
                    _mapper.Map(dtoEstimate, dbEstimate);
                    dbEstimate.UpdatedAt = DateTime.UtcNow;

                    CollectionSyncHelper.Sync(
                        dbCollection: dbEstimate.Items,
                        dtoCollection: dtoEstimate.Items,
                        keySelector: item => item.Id,
                        updateAction: (dbItem, dtoItem) => _mapper.Map(dtoItem, dbItem),
                        addAction: dtoItem => _mapper.Map<Dal.DbModels.EstimateItem>(dtoItem),
                        mapper: _mapper
                    );
                },
                addAction: dtoEstimate =>
                {
                    var newDbEstimate = _mapper.Map<Dal.DbModels.Estimate>(dtoEstimate);
                    newDbEstimate.CreatedAt = DateTime.UtcNow;
                    newDbEstimate.UpdatedAt = DateTime.UtcNow;
                    return newDbEstimate;
                },
                mapper: _mapper
            );

            return existingProject;
        }

        /// <summary>
        /// Добавляет или обновляет список проектов в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей проектов</param>
        /// <returns>Список идентификаторов сохраненных проектов</returns>
        protected override async Task<IList<Dal.DbModels.Project>> AddOrUpdateInternalAsync(IList<Entities.Project> entities)
        {
            try
            {
                if (entities == null)
                {
                    _logger.LogWarning("Попытка массового добавления/обновления проектов с null списком");
                    throw new ArgumentNullException(nameof(entities));
                }
                if (!entities.Any())
                    return new List<Dal.DbModels.Project>();

                var projects = new List<Dal.DbModels.Project>();
                foreach (var entity in entities)
                {
                    projects.Add(await AddOrUpdateInternalAsync(entity));
                }
                _logger.LogInformation("Массовое добавление/обновление проектов завершено. Всего: {Count}", projects.Count);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при массовом добавлении/обновлении проектов");
                throw;
            }
        }

        /// <summary>
        /// Выполняет обновление объекта перед сохранением
        /// </summary>
        /// <param name="entity">Сущность проекта</param>
        /// <param name="dbObject">DB модель проекта</param>
        /// <param name="exists">Флаг существования проекта в БД</param>
        protected override Task UpdateBeforeSavingAsync(Entities.Project entity, Dal.DbModels.Project dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.Now;
            }
            dbObject.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Строит список сущностей проектов на основе запроса к базе данных
        /// </summary>
        /// <param name="dbObjects">Запрос к БД</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Список сущностей проектов</returns>
        protected override async Task<IList<Entities.Project>> BuildEntitiesListAsync(IQueryable<Dal.DbModels.Project> dbObjects, bool isFull)
        {
            try
            {
                var query = dbObjects.AsNoTracking();
                if (isFull)
                {
                    query = query.Include(p => p.Client)
                                 .ThenInclude(c => c.User)
                                 .Include(p => p.Estimates)
                                 .ThenInclude(e => e.Items);
                }
                var dbProjects = await query.ToListAsync();
                var entities = dbProjects.Select(MapToEntity).ToList();
                _logger.LogDebug("Построен список проектов, количество: {Count}", entities.Count);
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении списка проектов");
                throw;
            }
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.Project> BuildDbQuery(ProjectSearchParams searchParams)
        {
            try
            {
                IQueryable<Dal.DbModels.Project> query = _context.Projects
                    .Include(p => p.Client);

                if (!searchParams.UserId.HasValue)
                {
                    _logger.LogWarning("Поиск проектов без указания UserId");
                    throw new ArgumentNullException(nameof(searchParams.UserId));
                }
                query = query.Where(p => p.Client.UserId == searchParams.UserId.Value);

                if (!string.IsNullOrEmpty(searchParams.Name))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(searchParams.Name.ToLower()));
                }

                if (searchParams.ClientId.HasValue)
                {
                    query = query.Where(p => p.ClientId == searchParams.ClientId.Value);
                }

                if (searchParams.Status.HasValue)
                {
                    query = query.Where(p => p.Status == searchParams.Status.Value);
                }
                _logger.LogDebug("Сформирован запрос к БД для поиска проектов");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при формировании запроса к БД для поиска проектов");
                throw;
            }
        }
        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.Project> query)
        {
            try
            {
                int count = await query.CountAsync();
                _logger.LogDebug("Подсчитано количество проектов: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете количества проектов");
                throw;
            }
        }

        /// <summary>
        /// Сохраняет изменения в базе данных
        /// </summary>
        protected override async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogDebug("Изменения в базе данных успешно сохранены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении изменений в базе данных");
                throw;
            }
        }

        /// <summary>
        /// Выполняет действие с использованием транзакции
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="action">Действие для выполнения</param>
        /// <returns>Результат действия</returns>
        protected override async Task<T> ExecuteWithTransactionAsync<T>(Func<Task<T>> action)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await transaction.CommitAsync();
                _logger.LogDebug("Транзакция успешно завершена");
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка в транзакции. Транзакция откатилась.");
                throw;
            }
        }

        /// <summary>
        /// Удаляет записи, соответствующие предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>True, если удаление выполнено успешно, иначе False</returns>
        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.Project, bool>> predicate)
        {
            try
            {
                var projects = await _context.Projects.Where(predicate).ToListAsync();
                if (!projects.Any())
                {
                    _logger.LogDebug("Удаление проектов: ни одной записи не найдено.");
                    return false;
                }

                _context.Projects.RemoveRange(projects);
                await SaveChangesAsync();
                _logger.LogInformation("Удалено проектов: {Count}", projects.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении проектов");
                throw;
            }
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.Project> Where(Expression<Func<Dal.DbModels.Project, bool>> predicate)
        {
            try
            {
                var query = _context.Projects.Where(predicate);
                _logger.LogDebug("Построен Where-запрос для проектов");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении Where-запроса для проектов");
                throw;
            }
        }

        /// <summary>
        /// Возвращает выражение для получения ID из DB модели
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Dal.DbModels.Project, long>> GetIdByDbObjectExpression() => p => p.Id;

        /// <summary>
        /// Возвращает выражение для получения ID из сущности
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Entities.Project, long>> GetIdByEntityExpression() => p => p.Id;

        /// <summary>
        /// Применяет сортировку по умолчанию
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Отсортированный запрос</returns>
        protected override IQueryable<Dal.DbModels.Project> ApplyDefaultSorting(IQueryable<Dal.DbModels.Project> query)
        {
            var sorted = query.OrderBy(p => p.CreatedAt);
            _logger.LogDebug("Применена сортировка по CreatedAt");
            return sorted;
        }
    }
}
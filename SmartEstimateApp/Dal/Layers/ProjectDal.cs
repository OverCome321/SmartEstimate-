using AutoMapper;
using Common.Convert;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dal.Layers
{
    /// <summary>
    /// Класс доступа к данным проектов
    /// </summary>
    public class ProjectDal : BaseDal<Dal.DbModels.Project, Entities.Project, long, ProjectSearchParams, ConvertParams>, IProjectDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор класса ProjectDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования между моделями</param>
        public ProjectDal(SmartEstimateDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
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
        public async Task<bool> ExistsAsync(long id) => await _context.Projects.AnyAsync(p => p.Id == id);

        /// <summary>
        /// Проверяет существование проекта по имени и клиенту
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>True, если проект с таким именем существует для указанного клиента, иначе False</returns>
        public async Task<bool> ExistsAsync(string name, long clientId) =>
            await _context.Projects.AnyAsync(p => p.Name == name && p.ClientId == clientId);

        /// <summary>
        /// Проверяет существование проекта по имени, клиенту и проверяет доступ пользователя
        /// </summary>
        /// <param name="name">Название проекта</param>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя для проверки доступа</param>
        /// <returns>True, если проект с таким именем существует и доступен пользователю, иначе False</returns>
        public async Task<bool> ExistsAsync(string name, long clientId, long userId) =>
            await _context.Projects
                .Include(p => p.Client)
                .AnyAsync(p => p.Name == name && p.ClientId == clientId && p.Client.UserId == userId);

        protected override async Task<long> AddOrUpdateInternalAsync(Entities.Project entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var dbProject = MapToDbProject(entity);

            var existingProject = await _context.Projects
                .Include(p => p.Estimates).ThenInclude(e => e.Items)
                .FirstOrDefaultAsync(p => p.Id == dbProject.Id);

            if (existingProject != null)
            {
                existingProject.Name = dbProject.Name;
                existingProject.Description = dbProject.Description;
                existingProject.Status = dbProject.Status;
                await UpdateBeforeSavingAsync(entity, existingProject, true);

                var existingEstimates = existingProject.Estimates.ToList();
                var newEstimateIds = entity.Estimates
                    .Where(e => e.Id > 0)
                    .Select(e => e.Id)
                    .ToList();
                var estimatesToRemove = existingEstimates
                    .Where(e => !newEstimateIds.Contains(e.Id))
                    .ToList();
                _context.Estimates.RemoveRange(estimatesToRemove);

                foreach (var estimate in entity.Estimates)
                {
                    var dbEstimate = _mapper.Map<Dal.DbModels.Estimate>(estimate);
                    dbEstimate.ProjectId = dbProject.Id;
                    dbEstimate.UpdatedAt = DateTime.UtcNow;

                    if (dbEstimate.Id > 0 && existingEstimates.Any(e => e.Id == dbEstimate.Id))
                    {
                        var existingEstimate = existingEstimates.First(e => e.Id == dbEstimate.Id);
                        _context.Entry(existingEstimate).CurrentValues.SetValues(dbEstimate);

                        var existingItems = existingEstimate.Items.ToList();
                        var newItemIds = estimate.Items.Where(i => i.Id > 0).Select(i => i.Id).ToList();
                        var itemsToRemove = existingItems.Where(i => !newItemIds.Contains(i.Id)).ToList();
                        _context.EstimateItems.RemoveRange(itemsToRemove);

                        foreach (var item in estimate.Items)
                        {
                            var dbItem = _mapper.Map<Dal.DbModels.EstimateItem>(item);
                            dbItem.EstimateId = dbEstimate.Id;
                            if (dbItem.Id > 0 && existingItems.Any(i => i.Id == dbItem.Id))
                            {
                                var existingItem = existingItems.First(i => i.Id == dbItem.Id);
                                _context.Entry(existingItem).CurrentValues.SetValues(dbItem);
                            }
                            else
                            {
                                await _context.EstimateItems.AddAsync(dbItem);
                            }
                        }
                    }
                    else
                    {
                        dbEstimate.CreatedAt = DateTime.UtcNow;
                        await _context.Estimates.AddAsync(dbEstimate);
                        foreach (var item in estimate.Items)
                        {
                            var dbItem = _mapper.Map<Dal.DbModels.EstimateItem>(item);
                            dbItem.EstimateId = dbEstimate.Id;
                            await _context.EstimateItems.AddAsync(dbItem);
                        }
                    }
                }

                _context.Projects.Update(existingProject);
            }
            else
            {
                await UpdateBeforeSavingAsync(entity, dbProject, false);
                await _context.Projects.AddAsync(dbProject);

                foreach (var estimate in entity.Estimates)
                {
                    var dbEstimate = _mapper.Map<Dal.DbModels.Estimate>(estimate);
                    dbEstimate.ProjectId = dbProject.Id;
                    dbEstimate.CreatedAt = DateTime.UtcNow;
                    await _context.Estimates.AddAsync(dbEstimate);
                    foreach (var item in estimate.Items)
                    {
                        var dbItem = _mapper.Map<Dal.DbModels.EstimateItem>(item);
                        dbItem.EstimateId = dbEstimate.Id;
                        await _context.EstimateItems.AddAsync(dbItem);
                    }
                }
            }
            return dbProject.Id;
        }

        /// <summary>
        /// Добавляет или обновляет список проектов в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей проектов</param>
        /// <returns>Список идентификаторов сохраненных проектов</returns>
        protected override async Task<IList<long>> AddOrUpdateInternalAsync(IList<Entities.Project> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (!entities.Any())
                return new List<long>();

            var ids = new List<long>();
            foreach (var entity in entities)
            {
                ids.Add(await AddOrUpdateInternalAsync(entity));
            }

            return ids;
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
            var query = dbObjects.AsNoTracking();
            if (isFull)
            {
                query = query.Include(p => p.Client)
                             .ThenInclude(c => c.User)
                             .Include(p => p.Estimates)
                             .ThenInclude(e => e.Items);
            }
            var dbProjects = await query.ToListAsync();
            return dbProjects.Select(MapToEntityProject).ToList();
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.Project> BuildDbQuery(ProjectSearchParams searchParams)
        {
            IQueryable<Dal.DbModels.Project> query = _context.Projects
                .Include(p => p.Client);

            // Фильтрация по UserId через Client.UserId
            if (!searchParams.UserId.HasValue)
            {
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

            return query;
        }

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.Project> query) => await query.CountAsync();

        /// <summary>
        /// Сохраняет изменения в базе данных
        /// </summary>
        protected override async Task SaveChangesAsync() => await _context.SaveChangesAsync();

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
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
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
            var projects = await _context.Projects.Where(predicate).ToListAsync();
            if (!projects.Any())
                return false;

            _context.Projects.RemoveRange(projects);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.Project> Where(Expression<Func<Dal.DbModels.Project, bool>> predicate) => _context.Projects.Where(predicate);

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
        protected override IQueryable<Dal.DbModels.Project> ApplyDefaultSorting(IQueryable<Dal.DbModels.Project> query) => query.OrderBy(p => p.CreatedAt);

        /// <summary>
        /// Преобразует сущность в DB модель
        /// </summary>
        /// <param name="entity">Сущность проекта</param>
        /// <returns>DB модель проекта</returns>
        private Dal.DbModels.Project MapToDbProject(Entities.Project entity) => _mapper.Map<Dal.DbModels.Project>(entity);

        /// <summary>
        /// Преобразует DB модель в сущность
        /// </summary>
        /// <param name="dbProject">DB модель проекта</param>
        /// <returns>Сущность проекта</returns>
        private Entities.Project MapToEntityProject(Dal.DbModels.Project dbProject) => _mapper.Map<Entities.Project>(dbProject);
    }
}
using AutoMapper;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dal.Layers
{
    /// <summary>
    /// Класс доступа к данным клиентов
    /// </summary>
    public class ClientDal : BaseDal<Dal.DbModels.Client, Entities.Client, long, ClientSearchParams, object>, IClientDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly ILogger<ClientDal> _logger;

        /// <summary>
        /// Конструктор класса ClientDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="logger">Логгер</param>
        public ClientDal(SmartEstimateDbContext context, IMapper mapper, ILogger<ClientDal> logger) : base(mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Определяет, требуются ли дополнительные обновления после сохранения объекта
        /// </summary>
        protected override bool RequiresUpdatesAfterObjectSaving => false;

        /// <summary>
        /// Проверяет существование клиента по ID и UserId
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            try
            {
                bool exists = await _context.Clients.AnyAsync(c => c.Id == id);
                _logger.LogDebug("Проверка существования клиента по Id={Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование клиента по email и UserId
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким email существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsAsync(string email, long userId)
        {
            try
            {
                bool exists = await _context.Clients.AnyAsync(c => c.Email == email && c.UserId == userId);
                _logger.LogDebug("Проверка существования клиента по Email={Email}, UserId={UserId}: {Exists}", email, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Email={Email}, UserId={UserId})", email, userId);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование клиента по телефону и UserId
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким телефоном существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsPhoneAsync(string phone, long userId)
        {
            try
            {
                bool exists = await _context.Clients.AnyAsync(c => c.Phone == phone && c.UserId == userId);
                _logger.LogDebug("Проверка существования клиента по Phone={Phone}, UserId={UserId}: {Exists}", phone, userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsPhoneAsync(Phone={Phone}, UserId={UserId})", phone, userId);
                throw;
            }
        }

        /// <summary>
        /// Добавляет или обновляет клиента в базе данных
        /// </summary>
        /// <param name="entity">Сущность клиента</param>
        /// <returns>Идентификатор сохраненного клиента</returns>
        protected override async Task<Dal.DbModels.Client> AddOrUpdateInternalAsync(Entities.Client entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Попытка добавить или обновить клиента с null entity");
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var existingClient = entity.Id > 0 ? await _context.Clients.FindAsync(entity.Id) : null;

                if (existingClient != null)
                {
                    _logger.LogInformation("Обновление клиента с Id={ClientId}", entity.Id);
                    _mapper.Map(entity, existingClient);
                    await UpdateBeforeSavingAsync(entity, existingClient, true);
                    return existingClient;
                }
                else
                {
                    _logger.LogInformation("Добавление нового клиента: {ClientName}", entity.Name);
                    var newDbClient = _mapper.Map<Dal.DbModels.Client>(entity);
                    await UpdateBeforeSavingAsync(entity, newDbClient, false);
                    await _context.Clients.AddAsync(newDbClient);

                    return newDbClient;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении клиента с Id={ClientId} и Name={ClientName}", entity.Id, entity.Name);
                throw;
            }
        }

        /// <summary>
        /// Добавляет или обновляет список клиентов в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей клиентов</param>
        /// <returns>Список идентификаторов сохраненных клиентов</returns>
        protected override async Task<IList<Dal.DbModels.Client>> AddOrUpdateInternalAsync(IList<Entities.Client> entities)
        {
            try
            {
                if (entities == null)
                {
                    _logger.LogWarning("Попытка массового добавления/обновления с null списком");
                    throw new ArgumentNullException(nameof(entities));
                }
                if (!entities.Any())
                    return new List<Dal.DbModels.Client>();

                var clients = new List<Dal.DbModels.Client>();
                foreach (var entity in entities)
                {
                    clients.Add(await AddOrUpdateInternalAsync(entity));
                }
                _logger.LogInformation("Массовое добавление/обновление клиентов завершено. Всего: {Count}", clients.Count);
                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при массовом добавлении/обновлении клиентов");
                throw;
            }
        }

        /// <summary>
        /// Выполняет обновление объекта перед сохранением
        /// </summary>
        /// <param name="entity">Сущность клиента</param>
        /// <param name="dbObject">DB модель клиента</param>
        /// <param name="exists">Флаг существования клиента в БД</param>
        protected override Task UpdateBeforeSavingAsync(Entities.Client entity, Dal.DbModels.Client dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.Now;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Строит список сущностей клиентов на основе запроса к базе данных
        /// </summary>
        /// <param name="dbObjects">Запрос к БД</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Список сущностей клиентов</returns>
        protected override async Task<IList<Entities.Client>> BuildEntitiesListAsync(IQueryable<Dal.DbModels.Client> dbObjects, bool isFull)
        {
            try
            {
                var query = dbObjects.AsNoTracking();
                var dbClients = await query.ToListAsync();
                var entities = dbClients.Select(MapToEntity).ToList();
                _logger.LogDebug("Построен список клиентов, количество: {Count}", entities.Count);
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении списка клиентов");
                throw;
            }
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.Client> BuildDbQuery(ClientSearchParams searchParams)
        {
            try
            {
                IQueryable<Dal.DbModels.Client> query = _context.Clients;

                if (!searchParams.UserId.HasValue)
                    throw new ArgumentNullException(nameof(searchParams.UserId));

                query = query.Where(c => c.UserId == searchParams.UserId.Value);

                // Единый поисковый запрос
                if (!string.IsNullOrWhiteSpace(searchParams.Name) ||
                    !string.IsNullOrWhiteSpace(searchParams.Email) ||
                    !string.IsNullOrWhiteSpace(searchParams.Phone))
                {
                    var name = searchParams.Name?.ToLower() ?? "";
                    var email = searchParams.Email?.ToLower() ?? "";
                    var phone = searchParams.Phone?.ToLower() ?? "";

                    query = query.Where(c =>
                        (!string.IsNullOrWhiteSpace(name) && c.Name.ToLower().Contains(name)) ||
                        (!string.IsNullOrWhiteSpace(email) && c.Email.ToLower().Contains(email)) ||
                        (!string.IsNullOrWhiteSpace(phone) && c.Phone.ToLower().Contains(phone))
                    );
                }

                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при формировании запроса к БД для поиска клиентов");
                throw;
            }
        }

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.Client> query)
        {
            try
            {
                int count = await query.CountAsync();
                _logger.LogDebug("Подсчитано количество клиентов: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете количества клиентов");
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
        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.Client, bool>> predicate)
        {
            try
            {
                var clients = await _context.Clients.Where(predicate).ToListAsync();
                if (!clients.Any())
                {
                    _logger.LogDebug("Удаление клиентов: ни одной записи не найдено.");
                    return false;
                }

                _context.Clients.RemoveRange(clients);
                await SaveChangesAsync();
                _logger.LogInformation("Удалено клиентов: {Count}", clients.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиентов");
                throw;
            }
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.Client> Where(Expression<Func<Dal.DbModels.Client, bool>> predicate)
        {
            try
            {
                var query = _context.Clients.Where(predicate);
                _logger.LogDebug("Построен Where-запрос для клиентов");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении Where-запроса для клиентов");
                throw;
            }
        }

        /// <summary>
        /// Возвращает выражение для получения ID из DB модели
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Dal.DbModels.Client, long>> GetIdByDbObjectExpression() => c => c.Id;

        /// <summary>
        /// Возвращает выражение для получения ID из сущности
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Entities.Client, long>> GetIdByEntityExpression() => c => c.Id;

        /// <summary>
        /// Применяет сортировку по умолчанию
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Отсортированный запрос</returns>
        protected override IQueryable<Dal.DbModels.Client> ApplyDefaultSorting(IQueryable<Dal.DbModels.Client> query)
        {
            var sorted = query.OrderBy(c => c.CreatedAt);
            _logger.LogDebug("Применена сортировка по CreatedAt");
            return sorted;
        }
    }
}
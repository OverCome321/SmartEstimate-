using AutoMapper;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dal.Layers
{
    /// <summary>
    /// Класс доступа к данным клиентов
    /// </summary>
    public class ClientDal : BaseDal<Dal.DbModels.Client, Entities.Client, long, ClientSearchParams, object>, IClientDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор класса ClientDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования между моделями</param>
        public ClientDal(SmartEstimateDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
        }

        /// <summary>
        /// Определяет, требуются ли дополнительные обновления после сохранения объекта
        /// </summary>
        protected override bool RequiresUpdatesAfterObjectSaving => false;

        /// <summary>
        /// Проверяет существование клиента по ID для указанного пользователя
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>True, если клиент существует, иначе False</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            throw new InvalidOperationException("Method requires UserId to check existence.");
        }

        /// <summary>
        /// Проверяет существование клиента по ID и UserId
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsAsync(long id, long userId)
        {
            return await _context.Clients.AnyAsync(c => c.Id == id && c.UserId == userId);
        }

        /// <summary>
        /// Проверяет существование клиента по email для указанного пользователя
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <returns>True, если клиент с таким email существует, иначе False</returns>
        public async Task<bool> ExistsAsync(string email)
        {
            throw new InvalidOperationException("Method requires UserId to check existence.");
        }

        /// <summary>
        /// Проверяет существование клиента по email и UserId
        /// </summary>
        /// <param name="email">Email клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким email существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsAsync(string email, long userId)
        {
            return await _context.Clients.AnyAsync(c => c.Email == email && c.UserId == userId);
        }

        /// <summary>
        /// Проверяет существование клиента по телефону для указанного пользователя
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <returns>True, если клиент с таким телефоном существует, иначе False</returns>
        public async Task<bool> ExistsPhoneAsync(string phone)
        {
            throw new InvalidOperationException("Method requires UserId to check existence.");
        }

        /// <summary>
        /// Проверяет существование клиента по телефону и UserId
        /// </summary>
        /// <param name="phone">Телефон клиента</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True, если клиент с таким телефоном существует для указанного пользователя, иначе False</returns>
        public async Task<bool> ExistsPhoneAsync(string phone, long userId)
        {
            return await _context.Clients.AnyAsync(c => c.Phone == phone && c.UserId == userId);
        }

        /// <summary>
        /// Добавляет или обновляет клиента в базе данных
        /// </summary>
        /// <param name="entity">Сущность клиента</param>
        /// <returns>Идентификатор сохраненного клиента</returns>
        protected override async Task<long> AddOrUpdateInternalAsync(Entities.Client entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var dbClient = MapToDbClient(entity);
            bool exists = dbClient.Id > 0 && await ExistsAsync(dbClient.Id, dbClient.UserId);

            if (exists)
            {
                await UpdateBeforeSavingAsync(entity, dbClient, true);
                _context.Clients.Update(dbClient);
            }
            else
            {
                await UpdateBeforeSavingAsync(entity, dbClient, false);
                await _context.Clients.AddAsync(dbClient);
            }

            return dbClient.Id;
        }

        /// <summary>
        /// Добавляет или обновляет список клиентов в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей клиентов</param>
        /// <returns>Список идентификаторов сохраненных клиентов</returns>
        protected override async Task<IList<long>> AddOrUpdateInternalAsync(IList<Entities.Client> entities)
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
        /// <param name="entity">Сущность клиента</param>
        /// <param name="dbObject">DB модель клиента</param>
        /// <param name="exists">Флаг существования клиента в БД</param>
        protected override Task UpdateBeforeSavingAsync(Entities.Client entity, Dal.DbModels.Client dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.UtcNow;
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
        protected override async Task<IList<Entities.Client>> BuildEntitiesListAsync(IQueryable<Dal.DbModels.Client> dbObjects, object? convertParams, bool isFull)
        {
            var query = dbObjects.AsNoTracking();
            var dbClients = await query.ToListAsync();
            return dbClients.Select(MapToEntityClient).ToList();
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.Client> BuildDbQuery(ClientSearchParams searchParams)
        {
            IQueryable<Dal.DbModels.Client> query = _context.Clients;

            // Фильтрация по UserId обязательна
            if (!searchParams.UserId.HasValue)
            {
                throw new ArgumentException("UserId is required for client search.");
            }
            query = query.Where(c => c.UserId == searchParams.UserId.Value);

            if (!string.IsNullOrEmpty(searchParams.Name))
            {
                query = query.Where(c => c.Name.ToLower().Contains(searchParams.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchParams.Email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(searchParams.Email.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchParams.Phone))
            {
                query = query.Where(c => c.Phone.ToLower().Contains(searchParams.Phone.ToLower()));
            }

            return query;
        }

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.Client> query) => await query.CountAsync();

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
        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.Client, bool>> predicate)
        {
            var clients = await _context.Clients.Where(predicate).ToListAsync();
            if (!clients.Any())
                return false;

            _context.Clients.RemoveRange(clients);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.Client> Where(Expression<Func<Dal.DbModels.Client, bool>> predicate) => _context.Clients.Where(predicate);

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
        protected override IQueryable<Dal.DbModels.Client> ApplyDefaultSorting(IQueryable<Dal.DbModels.Client> query) => query.OrderBy(c => c.CreatedAt);

        /// <summary>
        /// Преобразует сущность в DB модель
        /// </summary>
        /// <param name="entity">Сущность клиента</param>
        /// <returns>DB модель клиента</returns>
        private Dal.DbModels.Client MapToDbClient(Entities.Client entity) => _mapper.Map<Dal.DbModels.Client>(entity);

        /// <summary>
        /// Преобразует DB модель в сущность
        /// </summary>
        /// <param name="dbClient">DB модель клиента</param>
        /// <returns>Сущность клиента</returns>
        private Entities.Client MapToEntityClient(Dal.DbModels.Client dbClient) => _mapper.Map<Entities.Client>(dbClient);
    }
}
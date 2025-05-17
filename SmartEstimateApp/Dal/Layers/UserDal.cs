using AutoMapper;
using Common.Convert;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Dal.Layers
{
    /// <summary>
    /// Класс доступа к данным пользователей
    /// </summary>
    public class UserDal : BaseDal<Dal.DbModels.User, Entities.User, long, UserSearchParams, ConvertParams>, IUserDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserDal> _logger;

        /// <summary>
        /// Конструктор класса UserDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования между моделями</param>
        /// <param name="logger">Логгер</param>
        public UserDal(SmartEstimateDbContext context, IMapper mapper, ILogger<UserDal> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Определяет, требуются ли дополнительные обновления после сохранения объекта
        /// </summary>
        protected override bool RequiresUpdatesAfterObjectSaving => false;

        /// <summary>
        /// Проверяет существование пользователя по ID
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>True, если пользователь существует, иначе False</returns>
        public async Task<bool> ExistsAsync(long id)
        {
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == id);
                _logger.LogDebug("Проверка существования пользователя по Id={Id}: {Exists}", id, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>True, если пользователь с таким email существует, иначе False</returns>
        public async Task<bool> ExistsAsync(string email)
        {
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Email == email);
                _logger.LogDebug("Проверка существования пользователя по Email={Email}: {Exists}", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при ExistsAsync(Email={Email})", email);
                throw;
            }
        }

        /// <summary>
        /// Проверяет существование роли по ID
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <returns>True, если роль существует, иначе False</returns>
        public async Task<bool> RoleExistsAsync(long roleId)
        {
            try
            {
                var exists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                _logger.LogDebug("Проверка существования роли по RoleId={RoleId}: {Exists}", roleId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при RoleExistsAsync(RoleId={RoleId})", roleId);
                throw;
            }
        }

        /// <summary>
        /// Получает пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<Entities.User> GetAsync(string email, bool isFull = true)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Попытка получить пользователя по пустому email");
                    throw new ArgumentNullException(nameof(email));
                }

                var query = Where(u => u.Email.ToLower() == email.ToLower()).Take(1);
                var user = (await BuildEntitiesListAsync(query, isFull)).FirstOrDefault();
                if (user == null)
                    _logger.LogInformation("Пользователь с Email={Email} не найден", email);
                else
                    _logger.LogDebug("Получен пользователь: {@User}", user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по Email={Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Добавляет или обновляет пользователя в базе данных
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <returns>Идентификатор сохраненного пользователя</returns>
        protected override async Task<long> AddOrUpdateInternalAsync(Entities.User entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("Попытка добавить или обновить пользователя с null entity");
                    throw new ArgumentNullException(nameof(entity));
                }

                var dbUser = MapToDbUser(entity);
                bool exists = dbUser.Id > 0 && await _context.Users.AnyAsync(u => u.Id == dbUser.Id);

                if (exists)
                {
                    await UpdateBeforeSavingAsync(entity, dbUser, true);
                    _context.Users.Update(dbUser);
                    _logger.LogInformation("Пользователь обновлен в базе: {@User}", dbUser);
                }
                else
                {
                    await UpdateBeforeSavingAsync(entity, dbUser, false);
                    await _context.Users.AddAsync(dbUser);
                    _logger.LogInformation("Пользователь добавлен в базу: {@User}", dbUser);
                }

                return dbUser.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении пользователя: {@User}", entity);
                throw;
            }
        }

        /// <summary>
        /// Добавляет или обновляет список пользователей в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей пользователей</param>
        /// <returns>Список идентификаторов сохраненных пользователей</returns>
        protected override async Task<IList<long>> AddOrUpdateInternalAsync(IList<Entities.User> entities)
        {
            try
            {
                if (entities == null)
                {
                    _logger.LogWarning("Попытка массового добавления/обновления пользователей с null списком");
                    throw new ArgumentNullException(nameof(entities));
                }
                if (!entities.Any())
                    return new List<long>();

                var ids = new List<long>();
                foreach (var entity in entities)
                {
                    ids.Add(await AddOrUpdateInternalAsync(entity));
                }

                _logger.LogInformation("Массовое добавление/обновление пользователей завершено. Всего: {Count}", ids.Count);
                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при массовом добавлении/обновлении пользователей");
                throw;
            }
        }

        /// <summary>
        /// Выполняет обновление объекта перед сохранением
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <param name="dbObject">DB модель пользователя</param>
        /// <param name="exists">Флаг существования пользователя в БД</param>
        protected override Task UpdateBeforeSavingAsync(Entities.User entity, Dal.DbModels.User dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.Now;
            }
            else
            {
                dbObject.PasswordHash = entity.PasswordHash;
                dbObject.LastLogin = DateTime.Now;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Строит список сущностей пользователей на основе запроса к базе данных
        /// </summary>
        /// <param name="dbObjects">Запрос к БД</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Список сущностей пользователей</returns>
        protected override async Task<IList<Entities.User>> BuildEntitiesListAsync(IQueryable<Dal.DbModels.User> dbObjects, bool isFull)
        {
            try
            {
                var query = dbObjects.AsNoTracking();

                if (isFull)
                {
                    query = query.Include(u => u.Role);
                }

                var dbUsers = await query.ToListAsync();
                var entities = dbUsers.Select(MapToEntityUser).ToList();
                _logger.LogDebug("Построен список пользователей, количество: {Count}", entities.Count);
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении списка пользователей");
                throw;
            }
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.User> BuildDbQuery(UserSearchParams searchParams)
        {
            try
            {
                IQueryable<Dal.DbModels.User> query = _context.Users;

                if (!string.IsNullOrEmpty(searchParams.Email))
                {
                    query = query.Where(u => u.Email.ToLower().Contains(searchParams.Email.ToLower()));
                }

                if (searchParams.RoleId.HasValue)
                {
                    query = query.Where(u => u.RoleId == searchParams.RoleId.Value);
                }

                _logger.LogDebug("Сформирован запрос к БД для поиска пользователей");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при формировании запроса к БД для поиска пользователей");
                throw;
            }
        }

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.User> query)
        {
            try
            {
                int count = await query.CountAsync();
                _logger.LogDebug("Подсчитано количество пользователей: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете количества пользователей");
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
        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.User, bool>> predicate)
        {
            try
            {
                var users = await _context.Users.Where(predicate).ToListAsync();
                if (!users.Any())
                {
                    _logger.LogDebug("Удаление пользователей: ни одной записи не найдено.");
                    return false;
                }

                _context.Users.RemoveRange(users);
                await SaveChangesAsync();
                _logger.LogInformation("Удалено пользователей: {Count}", users.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователей");
                throw;
            }
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.User> Where(Expression<Func<Dal.DbModels.User, bool>> predicate)
        {
            try
            {
                var query = _context.Users.Where(predicate);
                _logger.LogDebug("Построен Where-запрос для пользователей");
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при построении Where-запроса для пользователей");
                throw;
            }
        }

        /// <summary>
        /// Возвращает выражение для получения ID из DB модели
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Dal.DbModels.User, long>> GetIdByDbObjectExpression() => u => u.Id;

        /// <summary>
        /// Возвращает выражение для получения ID из сущности
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected override Expression<Func<Entities.User, long>> GetIdByEntityExpression() => u => u.Id;

        /// <summary>
        /// Применяет сортировку по умолчанию
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Отсортированный запрос</returns>
        protected override IQueryable<Dal.DbModels.User> ApplyDefaultSorting(IQueryable<Dal.DbModels.User> query)
        {
            var sorted = query.OrderBy(u => u.CreatedAt);
            _logger.LogDebug("Применена сортировка по CreatedAt");
            return sorted;
        }

        /// <summary>
        /// Преобразует сущность в DB модель
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <returns>DB модель пользователя</returns>
        private Dal.DbModels.User MapToDbUser(Entities.User entity) => _mapper.Map<Dal.DbModels.User>(entity);

        /// <summary>
        /// Преобразует DB модель в сущность
        /// </summary>
        /// <param name="dbUser">DB модель пользователя</param>
        /// <returns>Сущность пользователя</returns>
        private Entities.User MapToEntityUser(Dal.DbModels.User dbUser) => _mapper.Map<Entities.User>(dbUser);
    }
}
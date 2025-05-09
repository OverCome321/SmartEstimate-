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
    /// Класс доступа к данным пользователей
    /// </summary>
    public class UserDal : BaseDal<Dal.DbModels.User, Entities.User, long, UserSearchParams, ConvertParams>, IUserDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор класса UserDal
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования между моделями</param>
        public UserDal(SmartEstimateDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
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
        public async Task<bool> ExistsAsync(long id) => await _context.Users.AnyAsync(u => u.Id == id);

        /// <summary>
        /// Проверяет существование пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>True, если пользователь с таким email существует, иначе False</returns>
        public async Task<bool> ExistsAsync(string email) => await _context.Users.AnyAsync(u => u.Email == email);

        /// <summary>
        /// Проверяет существование роли по ID
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <returns>True, если роль существует, иначе False</returns>
        public async Task<bool> RoleExistsAsync(long roleId) => await _context.Roles.AnyAsync(r => r.Id == roleId);


        /// <summary>
        /// Получает пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<Entities.User> GetAsync(string email, bool isFull = true)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            var query = Where(u => u.Email.ToLower() == email.ToLower()).Take(1);
            return (await BuildEntitiesListAsync(query, isFull)).FirstOrDefault();
        }

        /// <summary>
        /// Добавляет или обновляет пользователя в базе данных
        /// </summary>
        /// <param name="entity">Сущность пользователя</param>
        /// <returns>Идентификатор сохраненного пользователя</returns>
        protected override async Task<long> AddOrUpdateInternalAsync(Entities.User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var dbUser = MapToDbUser(entity);
            bool exists = dbUser.Id > 0 && await _context.Users.AnyAsync(u => u.Id == dbUser.Id);

            if (exists)
            {
                await UpdateBeforeSavingAsync(entity, dbUser, true);
                _context.Users.Update(dbUser);
            }
            else
            {
                // При использовании идентификатора типа long с Identity, 
                // нет необходимости генерировать ID вручную
                await UpdateBeforeSavingAsync(entity, dbUser, false);
                await _context.Users.AddAsync(dbUser);
            }

            return dbUser.Id;
        }

        /// <summary>
        /// Добавляет или обновляет список пользователей в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей пользователей</param>
        /// <returns>Список идентификаторов сохраненных пользователей</returns>
        protected override async Task<IList<long>> AddOrUpdateInternalAsync(IList<Entities.User> entities)
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
            var query = dbObjects.AsNoTracking();

            if (isFull)
            {
                query = query.Include(u => u.Role);
            }

            var dbUsers = await query.ToListAsync();
            return dbUsers.Select(MapToEntityUser).ToList();
        }

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected override IQueryable<Dal.DbModels.User> BuildDbQuery(UserSearchParams searchParams)
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

            return query;
        }

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.User> query) => await query.CountAsync();

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
        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.User, bool>> predicate)
        {
            var users = await _context.Users.Where(predicate).ToListAsync();
            if (!users.Any())
                return false;

            _context.Users.RemoveRange(users);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected override IQueryable<Dal.DbModels.User> Where(Expression<Func<Dal.DbModels.User, bool>> predicate) => _context.Users.Where(predicate);

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
        protected override IQueryable<Dal.DbModels.User> ApplyDefaultSorting(IQueryable<Dal.DbModels.User> query) => query.OrderBy(u => u.CreatedAt);

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
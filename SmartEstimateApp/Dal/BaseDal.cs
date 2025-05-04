using Common.Search;
using System.Linq.Expressions;
using System.Reflection;

namespace Dal
{
    /// <summary>
    /// Базовый абстрактный класс для слоя доступа к данным
    /// </summary>
    /// <typeparam name="TDbObject">Тип модели базы данных</typeparam>
    /// <typeparam name="TEntity">Тип бизнес-сущности</typeparam>
    /// <typeparam name="TObjectId">Тип идентификатора объекта</typeparam>
    /// <typeparam name="TSearchParams">Тип параметров поиска</typeparam>
    /// <typeparam name="TConvertParams">Тип параметров конвертации</typeparam>
    public abstract class BaseDal<TDbObject, TEntity, TObjectId, TSearchParams, TConvertParams>
        where TDbObject : class, new()
        where TEntity : class
        where TSearchParams : BaseSearchParams
        where TConvertParams : class
    {
        /// <summary>
        /// Определяет, требуются ли дополнительные обновления после сохранения объекта
        /// </summary>
        protected abstract bool RequiresUpdatesAfterObjectSaving { get; }

        /// <summary>
        /// Конструктор базового класса DAL
        /// </summary>
        protected BaseDal()
        {
        }

        /// <summary>
        /// Добавляет или обновляет сущность в базе данных
        /// </summary>
        /// <param name="entity">Сущность для добавления или обновления</param>
        /// <returns>Идентификатор сохраненной сущности</returns>
        public virtual async Task<TObjectId> AddOrUpdateAsync(TEntity entity)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await AddOrUpdateInternalAsync(entity);
                await SaveChangesAsync();
                return result;
            });
        }

        /// <summary>
        /// Добавляет или обновляет список сущностей в базе данных
        /// </summary>
        /// <param name="entities">Список сущностей для добавления или обновления</param>
        /// <returns>Список идентификаторов сохраненных сущностей</returns>
        public virtual async Task<IList<TObjectId>> AddOrUpdateAsync(IList<TEntity> entities)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await AddOrUpdateInternalAsync(entities);
                await SaveChangesAsync();
                return result;
            });
        }

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <returns>Найденная сущность или null</returns>
        public virtual async Task<TEntity> GetAsync(TObjectId id, TConvertParams convertParams = null)
        {
            var query = Where(GetCheckDbObjectIdExpression(id)).Take(1);
            return (await BuildEntitiesListAsync(query, convertParams, true)).FirstOrDefault();
        }

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True, если сущность была удалена, иначе False</returns>
        public virtual async Task<bool> DeleteAsync(TObjectId id)
        {
            return await DeleteAsync(GetCheckDbObjectIdExpression(id));
        }

        /// <summary>
        /// Удаляет сущности по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>True, если сущности были удалены, иначе False</returns>
        protected virtual async Task<bool> DeleteAsync(Expression<Func<TDbObject, bool>> predicate)
        {
            throw new NotImplementedException("DeleteAsync with predicate must be implemented in derived class.");
        }

        /// <summary>
        /// Получает список сущностей по параметрам поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <returns>Результат поиска с пагинацией</returns>
        public virtual async Task<SearchResult<TEntity>> GetAsync(TSearchParams searchParams, TConvertParams? convertParams = null)
        {
            var query = BuildDbQuery(searchParams);
            query = ApplyDefaultSorting(query);

            var result = new SearchResult<TEntity>
            {
                Total = await CountAsync(query),
                RequestedObjectsCount = searchParams.ObjectsCount,
                RequestedStartIndex = searchParams.StartIndex,
                Objects = new List<TEntity>()
            };

            if (searchParams.ObjectsCount == 0)
                return result;

            query = query.Skip(searchParams.StartIndex);
            if (searchParams.ObjectsCount.HasValue)
                query = query.Take(searchParams.ObjectsCount.Value);

            result.Objects = await BuildEntitiesListAsync(query, convertParams, false);
            return result;
        }

        /// <summary>
        /// Внутренний метод для добавления или обновления сущности
        /// </summary>
        /// <param name="entity">Сущность для добавления или обновления</param>
        /// <returns>Идентификатор сохраненной сущности</returns>
        protected abstract Task<TObjectId> AddOrUpdateInternalAsync(TEntity entity);

        /// <summary>
        /// Внутренний метод для добавления или обновления списка сущностей
        /// </summary>
        /// <param name="entities">Список сущностей для добавления или обновления</param>
        /// <returns>Список идентификаторов сохраненных сущностей</returns>
        protected abstract Task<IList<TObjectId>> AddOrUpdateInternalAsync(IList<TEntity> entities);

        /// <summary>
        /// Обновляет объект перед сохранением
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <param name="dbObject">Модель базы данных</param>
        /// <param name="exists">Флаг существования записи в БД</param>
        protected abstract Task UpdateBeforeSavingAsync(TEntity entity, TDbObject dbObject, bool exists);

        /// <summary>
        /// Обновляет объект после сохранения
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <param name="dbObject">Модель базы данных</param>
        /// <param name="exists">Флаг существования записи в БД</param>
        protected virtual Task UpdateAfterSavingAsync(TEntity entity, TDbObject dbObject, bool exists) => Task.CompletedTask;

        /// <summary>
        /// Строит запрос к БД на основе параметров поиска
        /// </summary>
        /// <param name="searchParams">Параметры поиска</param>
        /// <returns>Запрос к БД</returns>
        protected abstract IQueryable<TDbObject> BuildDbQuery(TSearchParams searchParams);

        /// <summary>
        /// Строит список сущностей на основе запроса к базе данных
        /// </summary>
        /// <param name="dbObjects">Запрос к БД</param>
        /// <param name="convertParams">Параметры конвертации</param>
        /// <param name="isFull">Флаг полной загрузки</param>
        /// <returns>Список сущностей</returns>
        protected abstract Task<IList<TEntity>> BuildEntitiesListAsync(IQueryable<TDbObject> dbObjects, TConvertParams? convertParams, bool isFull);

        /// <summary>
        /// Подсчитывает количество записей, соответствующих запросу
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Количество записей</returns>
        protected abstract Task<int> CountAsync(IQueryable<TDbObject> query);

        /// <summary>
        /// Сохраняет изменения в базе данных
        /// </summary>
        protected abstract Task SaveChangesAsync();

        /// <summary>
        /// Выполняет действие с использованием транзакции
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="action">Действие для выполнения</param>
        /// <returns>Результат действия</returns>
        protected abstract Task<T> ExecuteWithTransactionAsync<T>(Func<Task<T>> action);

        /// <summary>
        /// Фильтрует записи по предикату
        /// </summary>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный запрос</returns>
        protected abstract IQueryable<TDbObject> Where(Expression<Func<TDbObject, bool>> predicate);

        /// <summary>
        /// Возвращает выражение для получения ID из DB модели
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected abstract Expression<Func<TDbObject, TObjectId>> GetIdByDbObjectExpression();

        /// <summary>
        /// Возвращает выражение для получения ID из сущности
        /// </summary>
        /// <returns>Выражение для получения ID</returns>
        protected abstract Expression<Func<TEntity, TObjectId>> GetIdByEntityExpression();

        /// <summary>
        /// Получает ID из DB модели
        /// </summary>
        /// <param name="dbObject">DB модель</param>
        /// <returns>ID объекта</returns>
        protected TObjectId GetIdByDbObject(TDbObject dbObject) => GetIdByDbObjectExpression().Compile()(dbObject);

        /// <summary>
        /// Получает ID из сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <returns>ID объекта</returns>
        protected TObjectId GetIdByEntity(TEntity entity) => GetIdByEntityExpression().Compile()(entity);

        /// <summary>
        /// Создает выражение для проверки соответствия ID объекта
        /// </summary>
        /// <param name="objectId">ID объекта</param>
        /// <returns>Выражение для проверки</returns>
        protected Expression<Func<TDbObject, bool>> GetCheckDbObjectIdExpression(TObjectId objectId)
        {
            var parameter = Expression.Parameter(typeof(TDbObject));
            var property = GetDbObjectIdMember();
            return Expression.Lambda<Func<TDbObject, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, property),
                    Expression.Constant(objectId)),
                parameter);
        }

        /// <summary>
        /// Применяет сортировку по умолчанию
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <returns>Отсортированный запрос</returns>
        protected virtual IQueryable<TDbObject> ApplyDefaultSorting(IQueryable<TDbObject> query)
        {
            return query.OrderBy(GetIdByDbObjectExpression());
        }

        /// <summary>
        /// Получает информацию о свойстве ID в DB модели
        /// </summary>
        /// <returns>Информация о свойстве</returns>
        protected virtual PropertyInfo GetDbObjectIdMember()
        {
            if (GetIdByDbObjectExpression().Body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                    return propertyInfo;
            }
            throw new InvalidOperationException("Invalid ID expression; expected a property.");
        }
    }
}
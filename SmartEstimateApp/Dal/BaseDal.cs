using Common.Search;
using System.Linq.Expressions;
using System.Reflection;

namespace Dal
{
    public abstract class BaseDal<TDbObject, TEntity, TObjectId, TSearchParams, TConvertParams>
        where TDbObject : class, new()
        where TEntity : class
        where TSearchParams : BaseSearchParams
        where TConvertParams : class
    {
        protected abstract bool RequiresUpdatesAfterObjectSaving { get; }

        protected BaseDal()
        {
        }

        public virtual async Task<TObjectId> AddOrUpdateAsync(TEntity entity)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await AddOrUpdateInternalAsync(entity);
                await SaveChangesAsync();
                return result;
            });
        }

        public virtual async Task<IList<TObjectId>> AddOrUpdateAsync(IList<TEntity> entities)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await AddOrUpdateInternalAsync(entities);
                await SaveChangesAsync();
                return result;
            });
        }

        public virtual async Task<TEntity> GetAsync(TObjectId id, TConvertParams convertParams = null)
        {
            var query = Where(GetCheckDbObjectIdExpression(id)).Take(1);
            return (await BuildEntitiesListAsync(query, convertParams, true)).FirstOrDefault();
        }

        public virtual async Task<bool> DeleteAsync(TObjectId id)
        {
            return await DeleteAsync(GetCheckDbObjectIdExpression(id));
        }

        protected virtual async Task<bool> DeleteAsync(Expression<Func<TDbObject, bool>> predicate)
        {
            throw new NotImplementedException("DeleteAsync with predicate must be implemented in derived class.");
        }

        public virtual async Task<SearchResult<TEntity>> GetAsync(TSearchParams searchParams, TConvertParams convertParams = null)
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

        protected abstract Task<TObjectId> AddOrUpdateInternalAsync(TEntity entity);
        protected abstract Task<IList<TObjectId>> AddOrUpdateInternalAsync(IList<TEntity> entities);
        protected abstract Task UpdateBeforeSavingAsync(TEntity entity, TDbObject dbObject, bool exists);
        protected virtual Task UpdateAfterSavingAsync(TEntity entity, TDbObject dbObject, bool exists) => Task.CompletedTask;
        protected abstract IQueryable<TDbObject> BuildDbQuery(TSearchParams searchParams);
        protected abstract Task<IList<TEntity>> BuildEntitiesListAsync(IQueryable<TDbObject> dbObjects, TConvertParams convertParams, bool isFull);
        protected abstract Task<int> CountAsync(IQueryable<TDbObject> query);
        protected abstract Task SaveChangesAsync();
        protected abstract Task<T> ExecuteWithTransactionAsync<T>(Func<Task<T>> action);
        protected abstract IQueryable<TDbObject> Where(Expression<Func<TDbObject, bool>> predicate);
        protected abstract Expression<Func<TDbObject, TObjectId>> GetIdByDbObjectExpression();
        protected abstract Expression<Func<TEntity, TObjectId>> GetIdByEntityExpression();

        protected TObjectId GetIdByDbObject(TDbObject dbObject) => GetIdByDbObjectExpression().Compile()(dbObject);
        protected TObjectId GetIdByEntity(TEntity entity) => GetIdByEntityExpression().Compile()(entity);

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

        protected virtual IQueryable<TDbObject> ApplyDefaultSorting(IQueryable<TDbObject> query)
        {
            return query.OrderBy(GetIdByDbObjectExpression());
        }

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
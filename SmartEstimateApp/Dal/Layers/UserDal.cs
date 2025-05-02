using Common.Convert;
using Common.Search;
using Dal.DbModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dal.Layers
{
    public class UserDal : BaseDal<User, User, Guid, UserSearchParams, UserConvertParams>
    {
        private readonly SmartEstimateDbContext _context;

        public UserDal(SmartEstimateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected override bool RequiresUpdatesAfterObjectSaving => false;

        protected override async Task<Guid> AddOrUpdateInternalAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            bool exists = await _context.Users.AnyAsync(u => u.Id == entity.Id);
            if (exists)
            {
                await UpdateBeforeSavingAsync(entity, entity, true);
                _context.Users.Update(entity);
            }
            else
            {
                entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
                entity.CreatedAt = DateTime.UtcNow;
                await UpdateBeforeSavingAsync(entity, entity, false);
                await _context.Users.AddAsync(entity);
            }

            return entity.Id;
        }

        protected override async Task<IList<Guid>> AddOrUpdateInternalAsync(IList<User> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var ids = new List<Guid>();
            foreach (var entity in entities)
            {
                ids.Add(await AddOrUpdateInternalAsync(entity));
            }

            return ids;
        }

        protected override async Task UpdateBeforeSavingAsync(User entity, User dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.UtcNow;
            }
        }

        protected override async Task<IList<User>> BuildEntitiesListAsync(IQueryable<User> dbObjects, UserConvertParams convertParams, bool isFull)
        {
            var query = dbObjects;
            if (convertParams?.IncludeRole == true || isFull)
            {
                query = query.Include(u => u.Role);
            }

            return await query.ToListAsync();
        }

        protected override IQueryable<User> BuildDbQuery(UserSearchParams searchParams)
        {
            IQueryable<User> query = _context.Users;

            if (!string.IsNullOrEmpty(searchParams.Email))
            {
                query = query.Where(u => u.Email.Contains(searchParams.Email));
            }

            if (searchParams.RoleId.HasValue)
            {
                query = query.Where(u => u.RoleId == searchParams.RoleId.Value);
            }

            return query;
        }

        protected override async Task<int> CountAsync(IQueryable<User> query)
        {
            return await query.CountAsync();
        }

        protected override async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

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

        protected override IQueryable<User> Where(Expression<Func<User, bool>> predicate)
        {
            return _context.Users.Where(predicate);
        }

        protected override Expression<Func<User, Guid>> GetIdByDbObjectExpression()
        {
            return u => u.Id;
        }

        protected override Expression<Func<User, Guid>> GetIdByEntityExpression()
        {
            return u => u.Id;
        }

        protected override async Task<bool> DeleteAsync(Expression<Func<User, bool>> predicate)
        {
            var users = await _context.Users.Where(predicate).ToListAsync();
            if (!users.Any())
                return false;

            _context.Users.RemoveRange(users);
            await SaveChangesAsync();
            return true;
        }

        protected override IQueryable<User> ApplyDefaultSorting(IQueryable<User> query)
        {
            return query.OrderBy(u => u.CreatedAt);
        }
    }
}

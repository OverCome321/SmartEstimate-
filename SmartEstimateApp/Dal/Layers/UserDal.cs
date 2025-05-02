using AutoMapper;
using Common.Convert;
using Common.Search;
using Dal.DbModels;
using Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dal.Layers
{
    public class UserDal : BaseDal<Dal.DbModels.User, Entities.User, Guid, UserSearchParams, UserConvertParams>, IUserDal
    {
        private readonly SmartEstimateDbContext _context;
        private readonly IMapper _mapper;
        public UserDal(SmartEstimateDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
        }
        protected override bool RequiresUpdatesAfterObjectSaving => false;

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            return await _context.Roles.AnyAsync(r => r.Id == roleId);
        }

        protected override async Task<Guid> AddOrUpdateInternalAsync(Entities.User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var dbUser = MapToDbUser(entity);
            bool exists = await _context.Users.AnyAsync(u => u.Id == dbUser.Id);
            if (exists)
            {
                await UpdateBeforeSavingAsync(entity, dbUser, true);
                _context.Users.Update(dbUser);
            }
            else
            {
                dbUser.Id = dbUser.Id == Guid.Empty ? Guid.NewGuid() : dbUser.Id;
                await UpdateBeforeSavingAsync(entity, dbUser, false);
                await _context.Users.AddAsync(dbUser);
            }

            return dbUser.Id;
        }

        protected override async Task<IList<Guid>> AddOrUpdateInternalAsync(IList<Entities.User> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (!entities.Any())
                return new List<Guid>();

            var ids = new List<Guid>();
            foreach (var entity in entities)
            {
                ids.Add(await AddOrUpdateInternalAsync(entity));
            }

            return ids;
        }

        protected override Task UpdateBeforeSavingAsync(Entities.User entity, Dal.DbModels.User dbObject, bool exists)
        {
            if (!exists)
            {
                dbObject.CreatedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        protected override async Task<IList<Entities.User>> BuildEntitiesListAsync(IQueryable<Dal.DbModels.User> dbObjects, UserConvertParams convertParams, bool isFull)
        {
            var query = dbObjects.AsNoTracking();

            if (convertParams?.IncludeRole == true || isFull)
            {
                query = query.Include(u => u.Role);
            }

            var dbUsers = await query.ToListAsync();
            return dbUsers.Select(MapToEntityUser).ToList();
        }

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

        protected override async Task<int> CountAsync(IQueryable<Dal.DbModels.User> query)
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

        protected override IQueryable<Dal.DbModels.User> Where(Expression<Func<Dal.DbModels.User, bool>> predicate)
        {
            return _context.Users.Where(predicate);
        }

        protected override Expression<Func<Dal.DbModels.User, Guid>> GetIdByDbObjectExpression()
        {
            return u => u.Id;
        }

        protected override Expression<Func<Entities.User, Guid>> GetIdByEntityExpression()
        {
            return u => u.Id;
        }

        protected override async Task<bool> DeleteAsync(Expression<Func<Dal.DbModels.User, bool>> predicate)
        {
            var users = await _context.Users.Where(predicate).ToListAsync();
            if (!users.Any())
                return false;

            _context.Users.RemoveRange(users);
            await SaveChangesAsync();
            return true;
        }

        protected override IQueryable<Dal.DbModels.User> ApplyDefaultSorting(IQueryable<Dal.DbModels.User> query)
        {
            return query.OrderBy(u => u.CreatedAt);
        }

        private Dal.DbModels.User MapToDbUser(Entities.User entity)
        {
            return _mapper.Map<Dal.DbModels.User>(entity);
        }

        private Entities.User MapToEntityUser(Dal.DbModels.User dbUser)
        {
            return _mapper.Map<Entities.User>(dbUser);
        }
    }
}
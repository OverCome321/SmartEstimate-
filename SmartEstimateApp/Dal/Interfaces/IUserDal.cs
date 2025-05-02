using Common.Convert;
using Common.Search;
using Entities;

namespace Dal.Interfaces
{
    public interface IUserDal
    {
        Task<Guid> AddOrUpdateAsync(User entity);
        Task<IList<Guid>> AddOrUpdateAsync(IList<User> entities);
        Task<User> GetAsync(Guid id, UserConvertParams? convertParams = null);
        Task<bool> DeleteAsync(Guid id);
        Task<SearchResult<User>> GetAsync(UserSearchParams searchParams, UserConvertParams? convertParams = null);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsAsync(string email);
        Task<bool> RoleExistsAsync(Guid roleId);
    }
}
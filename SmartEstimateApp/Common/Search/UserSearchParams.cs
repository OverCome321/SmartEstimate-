namespace Common.Search
{
    public class UserSearchParams : BaseSearchParams
    {
        public string? Email { get; set; }
        public long? RoleId { get; set; }

        public UserSearchParams(string? email = null, long? roleId = null)
        {
            Email = email;
            RoleId = roleId;
        }
    }
}

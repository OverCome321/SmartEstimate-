namespace Common.Search
{
    public class UserSearchParams : BaseSearchParams
    {
        public string? Email { get; set; }
        public Guid? RoleId { get; set; }

        public UserSearchParams(string? email = null, Guid? roleId = null)
        {
            Email = email;
            RoleId = roleId;
        }
    }
}

namespace Common.Search
{
    public class UserSearchParams : BaseSearchParams
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public Guid? RoleId { get; set; }
        public UserSearchParams(string? username = null, string? email = null, Guid? roleId = null)
        {
            Username = username;
            Email = email;
            RoleId = roleId;
        }
    }
}

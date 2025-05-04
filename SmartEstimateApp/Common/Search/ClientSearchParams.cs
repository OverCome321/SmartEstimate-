namespace Common.Search
{
    public class ClientSearchParams : BaseSearchParams
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public ClientSearchParams(string? name = null, string? email = null, string? roleId = null)
        {
            Name = name;
            Email = email;
            Phone = roleId;
        }
    }
}

namespace Common.Search
{
    public class ProjectSearchParams : BaseSearchParams
    {
        public long? UserId { get; set; }
        public string? Name { get; set; }
        public long? ClientId { get; set; }
        public int? Status { get; set; }
        public ProjectSearchParams(string? name = null, long? clientId = null, int? status = null, long? userId = null)
        {
            Name = name;
            ClientId = clientId;
            Status = status;
            UserId = userId;
        }
    }
}

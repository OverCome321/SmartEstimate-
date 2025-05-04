namespace Entities
{
    public class Client
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; }
    }
}

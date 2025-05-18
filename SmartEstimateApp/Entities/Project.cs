namespace Entities
{
    public class Project
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; }

        public Client Client { get; set; }

        public User User { get; set; }

        public List<Estimate> Estimates { get; set; } = new List<Estimate>();
    }
}

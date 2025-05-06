using System.ComponentModel.DataAnnotations;

namespace Dal.DbModels
{
    public class Project
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; }

        public long ClientId { get; set; }

        public Client Client { get; set; }

        public List<Estimate> Estimates { get; set; } = new List<Estimate>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public List<Estimate> Estimates { get; set; } = new List<Estimate>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Client
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Phone, MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; }
        public string ClientType { get; set; }

        public List<Estimate> Estimates { get; set; } = new List<Estimate>();
        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
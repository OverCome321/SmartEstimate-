using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Client
    {
        public long Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        [Phone, MaxLength(20)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }
    }
}
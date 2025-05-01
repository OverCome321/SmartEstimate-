using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(50)]
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        [Required]
        public string Action { get; set; }

        [MaxLength(500)]
        public string Details { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}

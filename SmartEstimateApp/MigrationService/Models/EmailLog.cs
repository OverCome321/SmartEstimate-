using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class EmailLog
    {
        public Guid Id { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Recipient { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } // If sending failed

        public Guid EstimateId { get; set; }
        public Estimate Estimate { get; set; }
    }
}

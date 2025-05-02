using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Estimate
    {
        public Guid Id { get; set; }

        [Required]
        public string Number { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ValidUntil { get; set; }

        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; }

        public string Status { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        public List<EstimateItem> Items { get; set; } = new List<EstimateItem>();

        public List<FileMetadata> Files { get; set; } = new List<FileMetadata>();
    }
}
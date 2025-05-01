using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class EstimateTemplate
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public decimal DefaultTaxRate { get; set; }
        public decimal DefaultDiscountRate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<TemplateItem> Items { get; set; } = new List<TemplateItem>();
    }
}

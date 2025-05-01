using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class TemplateItem
    {
        public Guid Id { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; }

        public decimal DefaultQuantity { get; set; }
        public decimal DefaultUnitPrice { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        public int DisplayOrder { get; set; }

        public Guid TemplateId { get; set; }
        public EstimateTemplate Template { get; set; }
    }
}

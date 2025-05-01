using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class FileMetadata
    {
        public Guid Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileType { get; set; } // e.g., "PDF", "Excel"

        [Required]
        public string StoragePath { get; set; } // e.g., cloud URL or local path

        public long FileSize { get; set; } // In bytes

        public DateTime CreatedAt { get; set; }

        public Guid? EstimateId { get; set; }
        public Estimate? Estimate { get; set; }
    }
}
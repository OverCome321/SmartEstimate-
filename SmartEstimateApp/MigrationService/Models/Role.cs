using System.ComponentModel.DataAnnotations;

namespace MigrationService.Models
{
    public class Role
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
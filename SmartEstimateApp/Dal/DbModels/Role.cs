using System.ComponentModel.DataAnnotations;

namespace Dal.DbModels
{
    public class Role
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

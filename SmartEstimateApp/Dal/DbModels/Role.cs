using System.ComponentModel.DataAnnotations;

namespace Dal.DbModels
{
    public class Role
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

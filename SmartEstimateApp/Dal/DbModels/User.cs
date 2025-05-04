using System.ComponentModel.DataAnnotations;

namespace Dal.DbModels
{
    public class User
    {
        public long Id { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public long RoleId { get; set; }

        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }
}
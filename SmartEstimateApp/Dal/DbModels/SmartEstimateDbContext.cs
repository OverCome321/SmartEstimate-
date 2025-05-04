using Microsoft.EntityFrameworkCore;

namespace Dal.DbModels
{
    /// <summary>
    /// Контекст базы данных для приложения SmartEstimate
    /// </summary>
    public class SmartEstimateDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }

        /// <summary>
        /// Конструктор контекста базы данных
        /// </summary>
        /// <param name="options">Опции для настройки контекста</param>
        public SmartEstimateDbContext(DbContextOptions<SmartEstimateDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Настройка модели для базы данных
        /// </summary>
        /// <param name="modelBuilder">Строитель модели</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .UseIdentityColumn();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastLogin).IsRequired(false);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .UseIdentityColumn();
                entity.Property(e => e.Name).IsRequired();

                entity.HasData(
                    new Role { Id = 1, Name = "User" },
                    new Role { Id = 2, Name = "Admin" }
                );
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .UseIdentityColumn();
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Email)
                      .HasMaxLength(100);
                entity.Property(e => e.Phone)
                      .HasMaxLength(20);
                entity.Property(e => e.Address)
                      .HasMaxLength(200);
                entity.Property(e => e.CreatedAt)
                      .IsRequired();
                entity.Property(e => e.UpdatedAt)
                      .IsRequired(false);
                entity.Property(e => e.Status)
                      .IsRequired();
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Clients)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.Email);
            });
        }
    }
}
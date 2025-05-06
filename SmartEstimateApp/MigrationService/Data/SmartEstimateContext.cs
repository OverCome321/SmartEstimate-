using Microsoft.EntityFrameworkCore;
using MigrationService.Models;

namespace MigrationService.Data
{
    /// <summary>
    /// Контекст базы данных для приложения SmartEstimate
    /// </summary>
    public class SmartEstimateContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        public DbSet<EstimateItem> EstimateItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Конструктор контекста базы данных
        /// </summary>
        /// <param name="options">Опции для настройки контекста</param>
        public SmartEstimateContext(DbContextOptions<SmartEstimateContext> options)
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

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired(false);
                entity.Property(e => e.Status).IsRequired();

                entity.HasOne(p => p.Client)
                      .WithMany()
                      .HasForeignKey(p => p.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Estimate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .UseIdentityColumn();
                entity.Property(e => e.Number)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.CreatedAt)
                      .IsRequired();
                entity.Property(e => e.UpdatedAt)
                      .IsRequired(false);
                entity.Property(e => e.ValidUntil)
                      .IsRequired(false);
                entity.Property(e => e.Subtotal)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate)
                      .IsRequired()
                      .HasColumnType("decimal(5,2)");
                entity.Property(e => e.TaxAmount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountRate)
                      .IsRequired()
                      .HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency)
                      .HasMaxLength(10);
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Estimates)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
                entity.HasIndex(e => e.Number).IsUnique();
            });

            modelBuilder.Entity<EstimateItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .UseIdentityColumn();
                entity.Property(e => e.Description)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(e => e.Quantity)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category)
                      .HasMaxLength(100);
                entity.Property(e => e.DisplayOrder)
                      .IsRequired();
                entity.HasOne(e => e.Estimate)
                      .WithMany(e => e.Items)
                      .HasForeignKey(e => e.EstimateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
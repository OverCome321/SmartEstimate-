using Microsoft.EntityFrameworkCore;
using MigrationService.Models;

namespace MigrationService.Data
{
    public class SmartEstimateContext : DbContext
    {
        public SmartEstimateContext(DbContextOptions options) : base(options) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        public DbSet<EstimateItem> EstimateItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureIdentityColumns(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Admin" }
            );

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Estimates)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Estimate>()
                .HasMany(e => e.Items)
                .WithOne(i => i.Estimate)
                .HasForeignKey(i => i.EstimateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Estimate>()
                .HasIndex(e => e.Number)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .Property(c => c.Status)
                .HasDefaultValue(1);

            modelBuilder.Entity<Estimate>()
                .Property(e => e.Status)
                .HasDefaultValue("Draft");

            modelBuilder.Entity<Estimate>()
                .Property(e => e.Currency)
                .HasDefaultValue("RUB");

            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasDefaultValue(1);
        }

        private void ConfigureIdentityColumns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<Estimate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<EstimateItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
            });
        }
    }
}
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
        public DbSet<EstimateTemplate> EstimateTemplates { get; set; }
        public DbSet<TemplateItem> TemplateItems { get; set; }
        public DbSet<FileMetadata> FileMetadata { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var idProperty = entityType.FindProperty("Id");
                if (idProperty != null && idProperty.ClrType == typeof(Guid))
                {
                    idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                    idProperty.SetDefaultValueSql("NEWID()");
                }
            }
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Существующие связи
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Estimates)
                .WithOne(e => e.Client)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<EstimateTemplate>()
                .HasMany(t => t.Items)
                .WithOne(ti => ti.Template)
                .HasForeignKey(ti => ti.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Estimate>()
                .HasMany(e => e.Files)
                .WithOne(f => f.Estimate)
                .HasForeignKey(f => f.EstimateId)
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
                .HasDefaultValue("Active");

            modelBuilder.Entity<Client>()
                .Property(c => c.ClientType)
                .HasDefaultValue("Individual");

            modelBuilder.Entity<Estimate>()
                .Property(e => e.Status)
                .HasDefaultValue("Draft");

            modelBuilder.Entity<Estimate>()
                .Property(e => e.Currency)
                .HasDefaultValue("RUB");

            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasDefaultValue("Active");
        }
    }
}
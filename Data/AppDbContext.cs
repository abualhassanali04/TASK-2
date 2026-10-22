using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data.Conventions;
using ProductCatalogApi.Models;

namespace ProductCatalogApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Every DateTime column is datetime2 and holds UTC.
            configurationBuilder.Properties<DateTime>()
                .HaveColumnType("datetime2")
                .HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>()
                .HaveColumnType("datetime2")
                .HaveConversion<NullableUtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Audit columns (CreatedAt/CreatedBy/UpdatedAt/UpdatedBy) on every IAuditable table.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(IAuditable).IsAssignableFrom(t.ClrType)))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                // Defaults only matter for rows that already exist when the migration runs;
                // new rows are stamped by AuditInterceptor.
                entity.Property(nameof(IAuditable.CreatedAt))
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(nameof(IAuditable.CreatedBy))
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasDefaultValue(AuditInterceptor.SystemUser);
                entity.Property(nameof(IAuditable.UpdatedBy))
                    .HasMaxLength(100);
            }

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}

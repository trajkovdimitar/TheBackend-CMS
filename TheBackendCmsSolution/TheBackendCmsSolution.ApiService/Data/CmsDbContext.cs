using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.ApiService.Models;

namespace TheBackendCmsSolution.ApiService.Data;

public class CmsDbContext : DbContext
{
    public CmsDbContext(DbContextOptions<CmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContentItem> ContentItems { get; set; }
    public DbSet<ContentType> ContentTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasOne(e => e.ContentType)
                  .WithMany(ct => ct.ContentItems)
                  .HasForeignKey(e => e.Type)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for now
        });

        modelBuilder.Entity<ContentType>(entity =>
        {
            entity.HasKey(ct => ct.Name);
            entity.Property(ct => ct.Name).IsRequired().HasMaxLength(50);
            entity.Property(ct => ct.DisplayName).IsRequired().HasMaxLength(100);
        });
    }
}
using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.ApiService.Models;

namespace TheBackendCmsSolution.ApiService.Data
{
    public class CmsDbContext : DbContext
    {
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }

        public CmsDbContext(DbContextOptions<CmsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentType>()
                .Property(c => c.Fields)
                .HasColumnType("jsonb");
            modelBuilder.Entity<ContentItem>()
                .Property(c => c.Fields)
                .HasColumnType("jsonb");
            modelBuilder.Entity<ContentItem>()
                .HasOne(c => c.ContentType)
                .WithMany()
                .HasForeignKey(c => c.ContentTypeId);
        }
    }
}
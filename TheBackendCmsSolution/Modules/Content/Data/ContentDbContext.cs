using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Content.Models;

namespace TheBackendCmsSolution.Modules.Content.Data
{
    public class ContentDbContext : DbContext
    {
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }

        public ContentDbContext(DbContextOptions<ContentDbContext> options)
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

using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using TheBackendCmsSolution.Modules.OpenId.Models;

namespace TheBackendCmsSolution.Modules.OpenId.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict();
    }
}

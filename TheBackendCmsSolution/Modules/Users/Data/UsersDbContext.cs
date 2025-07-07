using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Users.Models;

namespace TheBackendCmsSolution.Modules.Users.Data;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }
}

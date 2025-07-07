using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheBackendCmsSolution.Modules.Users.Data;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        var connectionString = "Host=localhost;Port=5435;Database=usersdb;Username=postgres;Password=postgres";
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        return new UsersDbContext(optionsBuilder.Options);
    }
}

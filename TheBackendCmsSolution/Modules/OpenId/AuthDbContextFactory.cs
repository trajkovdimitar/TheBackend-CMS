using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TheBackendCmsSolution.Modules.OpenId.Data;

namespace TheBackendCmsSolution.Modules.OpenId;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = configuration.GetConnectionString("authdb") ??
            "Host=localhost;Port=5434;Database=authdb;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseOpenIddict();
        return new AuthDbContext(optionsBuilder.Options);
    }
}

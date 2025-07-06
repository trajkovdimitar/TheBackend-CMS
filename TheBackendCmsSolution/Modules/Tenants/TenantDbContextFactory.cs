using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TheBackendCmsSolution.Modules.Tenants.Data;

namespace TheBackendCmsSolution.Modules.Tenants;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        var connectionString = "Host=localhost;Port=5433;Database=tenantsdb;Username=postgres;Password=postgres";
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        return new TenantDbContext(optionsBuilder.Options);
    }
}

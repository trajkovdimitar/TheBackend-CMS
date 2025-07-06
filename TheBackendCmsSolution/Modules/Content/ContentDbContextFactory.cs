using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TheBackendCmsSolution.Modules.Content.Data;

namespace TheBackendCmsSolution.Modules.Content;

public class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        var connectionString = "Host=localhost;Port=5432;Database=contentdb;Username=postgres;Password=postgres";
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        return new ContentDbContext(optionsBuilder.Options);
    }
}

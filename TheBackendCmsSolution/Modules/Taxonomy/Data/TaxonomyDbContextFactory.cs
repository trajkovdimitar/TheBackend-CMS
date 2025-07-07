using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheBackendCmsSolution.Modules.Taxonomy.Data;

public class TaxonomyDbContextFactory : IDesignTimeDbContextFactory<TaxonomyDbContext>
{
    public TaxonomyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaxonomyDbContext>();
        var connectionString = "Host=localhost;Port=5434;Database=taxonomydb;Username=postgres;Password=postgres";
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        optionsBuilder.UseNpgsql(dataSourceBuilder.Build());
        return new TaxonomyDbContext(optionsBuilder.Options);
    }
}

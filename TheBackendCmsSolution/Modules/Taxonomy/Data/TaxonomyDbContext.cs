using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Taxonomy.Models;

namespace TheBackendCmsSolution.Modules.Taxonomy.Data;

public class TaxonomyDbContext : DbContext
{
    public DbSet<TaxonomyTerm> Terms => Set<TaxonomyTerm>();

    public TaxonomyDbContext(DbContextOptions<TaxonomyDbContext> options) : base(options)
    {
    }
}

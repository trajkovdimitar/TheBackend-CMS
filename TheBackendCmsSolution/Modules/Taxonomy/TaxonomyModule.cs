using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Taxonomy.Data;
using TheBackendCmsSolution.Modules.Taxonomy.Models;

namespace TheBackendCmsSolution.Modules.Taxonomy;

public class TaxonomyModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaxonomyDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("taxonomydb") ??
                              "Host=localhost;Port=5434;Database=taxonomydb;Username=postgres;Password=postgres";
            var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connection);
            dataSourceBuilder.EnableDynamicJson();
            options.UseNpgsql(dataSourceBuilder.Build());
        });
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/taxonomy");

        group.MapGet("/terms", async (TaxonomyDbContext db) => await db.Terms.ToListAsync());

        group.MapGet("/terms/{id:guid}", async (Guid id, TaxonomyDbContext db, ILogger<TaxonomyModule> logger) =>
        {
            var term = await db.Terms.FindAsync(id);
            if (term is null)
            {
                logger.LogWarning("Taxonomy term {Id} not found", id);
                return Results.NotFound();
            }
            return Results.Ok(term);
        });

        group.MapPost("/terms", async (TaxonomyTerm term, TaxonomyDbContext db, ILogger<TaxonomyModule> logger) =>
        {
            term.Id = Guid.NewGuid();
            term.CreatedAt = DateTime.UtcNow;
            db.Terms.Add(term);
            await db.SaveChangesAsync();
            logger.LogInformation("Created taxonomy term {Id}", term.Id);
            return Results.Created($"/taxonomy/terms/{term.Id}", term);
        });

        group.MapPut("/terms/{id:guid}", async (Guid id, TaxonomyTerm term, TaxonomyDbContext db, ILogger<TaxonomyModule> logger) =>
        {
            var existing = await db.Terms.FindAsync(id);
            if (existing is null)
            {
                logger.LogWarning("Taxonomy term {Id} not found for update", id);
                return Results.NotFound();
            }
            existing.Name = term.Name;
            existing.Slug = term.Slug;
            existing.Type = term.Type;
            await db.SaveChangesAsync();
            logger.LogInformation("Updated taxonomy term {Id}", id);
            return Results.Ok(existing);
        });

        group.MapDelete("/terms/{id:guid}", async (Guid id, TaxonomyDbContext db, ILogger<TaxonomyModule> logger) =>
        {
            var existing = await db.Terms.FindAsync(id);
            if (existing is null)
            {
                logger.LogWarning("Taxonomy term {Id} not found for deletion", id);
                return Results.NotFound();
            }
            db.Terms.Remove(existing);
            await db.SaveChangesAsync();
            logger.LogInformation("Deleted taxonomy term {Id}", id);
            return Results.NoContent();
        });
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaxonomyDbContext>();
        db.Database.Migrate();

        if (!db.Terms.Any())
        {
            db.Terms.AddRange(
                new TaxonomyTerm
                {
                    Id = Guid.NewGuid(),
                    Name = "Default Category",
                    Slug = "default-category",
                    Type = "category",
                    CreatedAt = DateTime.UtcNow
                },
                new TaxonomyTerm
                {
                    Id = Guid.NewGuid(),
                    Name = "General",
                    Slug = "general",
                    Type = "tag",
                    CreatedAt = DateTime.UtcNow
                });
            db.SaveChanges();
        }
    }
}

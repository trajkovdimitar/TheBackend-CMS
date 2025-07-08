using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Npgsql;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Tenants.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;
using TheBackendCmsSolution.Modules.Tenants.Services;
using System.Linq;

namespace TheBackendCmsSolution.Modules.Tenants;

public class TenancyModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TenantDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("tenantsdb") ??
                                   "Host=localhost;Port=5433;Database=tenantsdb;Username=postgres;Password=postgres";
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 20;
            options.UseNpgsql(dataSourceBuilder.Build());
        });
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantAccessor, TenantAccessor>();
        services.AddScoped<ITenantStore, TenantStore>();
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tenants");

        group.MapGet("/", async (ITenantStore store) => await store.GetAllAsync());

        group.MapGet("/{id:guid}", async (Guid id, ITenantStore store, ILogger<TenancyModule> logger) =>
        {
            var tenant = await store.GetAsync(id);
            if (tenant is null)
            {
                logger.LogWarning("Tenant {Id} not found", id);
                return Results.NotFound();
            }
            return Results.Ok(tenant);
        });

        group.MapPost("/", async (Tenant tenant, ITenantStore store, ILogger<TenancyModule> logger) =>
        {
            var created = await store.CreateAsync(tenant);
            logger.LogInformation("Created tenant {Id}", created.Id);
            return Results.Created($"/tenants/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (Guid id, Tenant tenant, ITenantStore store, ILogger<TenancyModule> logger) =>
        {
            var updated = await store.UpdateAsync(id, tenant);
            if (!updated)
            {
                logger.LogWarning("Tenant {Id} not found for update", id);
                return Results.NotFound();
            }
            logger.LogInformation("Updated tenant {Id}", id);
            tenant.Id = id;
            return Results.Ok(tenant);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ITenantStore store, ILogger<TenancyModule> logger) =>
        {
            var removed = await store.DeleteAsync(id);
            if (!removed)
            {
                logger.LogWarning("Tenant {Id} not found for deletion", id);
                return Results.NotFound();
            }
            logger.LogInformation("Deleted tenant {Id}", id);
            return Results.NoContent();
        });
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        db.Database.Migrate();

        if (!db.Tenants.Any())
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var defaultConn = configuration.GetConnectionString("contentdb") ??
                              "Host=localhost;Port=5432;Database=contentdb;Username=postgres;Password=postgres";
            db.Tenants.Add(new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Default",
                Host = "localhost",
                ConnectionString = defaultConn,
                EnabledModules = ModuleLoader.DiscoverModules()
                    .Select(m => m.GetType().FullName!)
                    .ToList()
            });
            db.SaveChanges();
        }
    }
}

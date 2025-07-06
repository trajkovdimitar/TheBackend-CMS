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

namespace TheBackendCmsSolution.Modules.Tenants;

public class TenancyModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("tenantsdb") ??
                               "Host=localhost;Port=5433;Database=tenantsdb;Username=postgres;Password=postgres";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        services.AddDbContext<TenantDbContext>(options => options.UseNpgsql(dataSourceBuilder.Build()));
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantAccessor, TenantAccessor>();
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tenants");
        group.MapGet("/", async (TenantDbContext db) => await db.Tenants.ToListAsync());
        group.MapPost("/", async (Tenant tenant, TenantDbContext db, ILogger<TenancyModule> logger) =>
        {
            tenant.Id = Guid.NewGuid();
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
            logger.LogInformation("Created tenant {Id}", tenant.Id);
            return Results.Created($"/tenants/{tenant.Id}", tenant);
        });
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        db.Database.Migrate();
    }
}

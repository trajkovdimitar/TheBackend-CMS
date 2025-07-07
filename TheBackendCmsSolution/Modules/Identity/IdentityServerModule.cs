using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheBackendCmsSolution.Modules.Abstractions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TheBackendCmsSolution.Modules.Identity.Data;

namespace TheBackendCmsSolution.Modules.Identity;

public class IdentityServerModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddDbContext<ApplicationIdentityDbContext>((sp, options) =>
        {
            var connString = configuration.GetConnectionString("identitydb") ??
                               "Host=localhost;Port=5434;Database=identitydb;Username=postgres;Password=postgres";
            var dsBuilder = new NpgsqlDataSourceBuilder(connString);
            dsBuilder.EnableDynamicJson();
            options.UseNpgsql(dsBuilder.Build());
        });

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddIdentityServer()
            .AddAspNetIdentity<IdentityUser>()
            .AddInMemoryApiScopes([
                new ApiScope("cms.api", "CMS API")
            ])
            .AddInMemoryClients([
                new Client
                {
                    ClientId = "cms.client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "cms.api" }
                }
            ])
            .AddDeveloperSigningCredential();

        services.AddScoped<Duende.IdentityServer.Services.IServerUrls, BasicServerUrls>();
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        if (endpoints is IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        db.Database.Migrate();
    }
}

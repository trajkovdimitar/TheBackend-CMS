using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenIddict.Abstractions;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Identity.Data;

namespace TheBackendCmsSolution.Modules.Identity;

public class OpenIdModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationIdentityDbContext>((sp, options) =>
        {
            var connString = configuration.GetConnectionString("identitydb") ??
                               "Host=localhost;Port=5434;Database=identitydb;Username=postgres;Password=postgres";
            var dsBuilder = new NpgsqlDataSourceBuilder(connString);
            dsBuilder.EnableDynamicJson();
            dsBuilder.ConnectionStringBuilder.MaxPoolSize = 20;
            options.UseNpgsql(dsBuilder.Build());
        });

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationIdentityDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.AllowPasswordFlow()
                       .AllowClientCredentialsFlow();
                options.AcceptAnonymousClients();
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();
                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough();
            });
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        if (endpoints is IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        db.Database.Migrate();
    }
}

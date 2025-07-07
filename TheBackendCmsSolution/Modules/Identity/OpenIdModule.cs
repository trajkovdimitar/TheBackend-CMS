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

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string adminRole = "Administrator";
        const string adminUser = "admin";
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin123!";

        if (!roleManager.Roles.Any(r => r.Name == adminRole))
        {
            roleManager.CreateAsync(new IdentityRole(adminRole)).GetAwaiter().GetResult();
        }

        if (userManager.FindByNameAsync(adminUser).GetAwaiter().GetResult() is null)
        {
            var user = new IdentityUser(adminUser) { Email = adminEmail, EmailConfirmed = true };
            userManager.CreateAsync(user, adminPassword).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(user, adminRole).GetAwaiter().GetResult();
        }
    }
}

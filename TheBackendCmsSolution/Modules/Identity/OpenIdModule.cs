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

        const string editorRole = "Editor";
        const string editorUser = "editor";
        const string editorEmail = "editor@example.com";
        const string editorPassword = "Editor123!";

        const string viewerRole = "Viewer";
        const string viewerUser = "viewer";
        const string viewerEmail = "viewer@example.com";
        const string viewerPassword = "Viewer123!";

        foreach (var role in new[] { adminRole, editorRole, viewerRole })
        {
            if (!roleManager.Roles.Any(r => r.Name == role))
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }
        }

        if (userManager.FindByNameAsync(adminUser).GetAwaiter().GetResult() is null)
        {
            var user = new IdentityUser(adminUser) { Email = adminEmail, EmailConfirmed = true };
            userManager.CreateAsync(user, adminPassword).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(user, adminRole).GetAwaiter().GetResult();
        }

        if (userManager.FindByNameAsync(editorUser).GetAwaiter().GetResult() is null)
        {
            var user = new IdentityUser(editorUser) { Email = editorEmail, EmailConfirmed = true };
            userManager.CreateAsync(user, editorPassword).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(user, editorRole).GetAwaiter().GetResult();
        }

        if (userManager.FindByNameAsync(viewerUser).GetAwaiter().GetResult() is null)
        {
            var user = new IdentityUser(viewerUser) { Email = viewerEmail, EmailConfirmed = true };
            userManager.CreateAsync(user, viewerPassword).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(user, viewerRole).GetAwaiter().GetResult();
        }

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (scopeManager.FindByNameAsync("cms.api").GetAwaiter().GetResult() is null)
        {
            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = "cms.api",
                DisplayName = "CMS API"
            };
            scopeManager.CreateAsync(descriptor).GetAwaiter().GetResult();
        }
    }
}

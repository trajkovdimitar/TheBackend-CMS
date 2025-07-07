using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheBackendCmsSolution.Modules.Abstractions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer;
using Duende.IdentityServer.Test;

namespace TheBackendCmsSolution.Modules.Identity;

public class IdentityServerModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityServer()
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
            .AddTestUsers([
                new TestUser
                {
                    SubjectId = "1",
                    Username = "admin",
                    Password = "password"
                }
            ])
            .AddDeveloperSigningCredential();
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
        // no-op
    }
}

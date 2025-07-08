using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Identity.Data;

namespace TheBackendCmsSolution.Modules.IdentityServer;

public class IdentityServerModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IServerUrls, BasicServerUrls>();

        services.AddIdentityServer()
            .AddAspNetIdentity<IdentityUser>()
            .AddInMemoryApiScopes([new ApiScope("cms.api", "CMS API")])
            .AddInMemoryClients([
                new Client
                {
                    ClientId = "cms.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "cms.api" }
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
        // no migrations
    }
}

internal class BasicServerUrls : IServerUrls
{
    private readonly IHttpContextAccessor _contextAccessor;

    public BasicServerUrls(IHttpContextAccessor contextAccessor) => _contextAccessor = contextAccessor;

    public string Origin { get; set; } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public string BaseUrl
    {
        get
        {
            if (string.IsNullOrEmpty(Origin) && _contextAccessor.HttpContext != null)
            {
                Origin = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}";
                BasePath = _contextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;
            }
            return Origin + BasePath;
        }
    }

    public string GetIdentityServerRelativeUrl(string path) => path.StartsWith("/") ? path : "/" + path;

    public string GetAbsoluteUrl(string path) => Origin + GetIdentityServerRelativeUrl(path);

    public IEnumerable<string> GetServerUrls() => new[] { BaseUrl };
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TheBackendCmsSolution.Modules.Abstractions;

namespace TheBackendCmsSolution.Modules.Taxonomy;

public class TaxonomyModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // register services for Taxonomy feature
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/taxonomy");
        group.MapGet("/", () => Results.Ok(new { Message = "Taxonomy module" }));
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        // apply taxonomy migrations
    }
}

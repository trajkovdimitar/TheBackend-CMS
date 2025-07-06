using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TheBackendCmsSolution.Modules.Abstractions;

namespace TheBackendCmsSolution.Modules.Users;

public class UsersModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // register services for Users feature
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/users");
        group.MapGet("/", () => Results.Ok(new { Message = "Users module" }));
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        // apply user migrations
    }
}

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheBackendCmsSolution.Modules.Abstractions;

public interface ICmsModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void MapRoutes(IEndpointRouteBuilder endpoints);
    void ApplyMigrations(IServiceProvider services);
}

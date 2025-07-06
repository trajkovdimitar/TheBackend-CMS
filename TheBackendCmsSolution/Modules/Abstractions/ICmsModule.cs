namespace TheBackendCmsSolution.Modules.Abstractions;

public interface ICmsModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void MapRoutes(IEndpointRouteBuilder endpoints);
    void ApplyMigrations(IServiceProvider services);
}

using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Tenants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var modules = ModuleLoader.DiscoverModules();

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

app.UseMiddleware<TheBackendCmsSolution.Modules.Tenants.TenantResolutionMiddleware>();

foreach (var module in modules)
{
    module.ApplyMigrations(app.Services);
    module.MapRoutes(app);
}

app.MapDefaultEndpoints();
app.Run();

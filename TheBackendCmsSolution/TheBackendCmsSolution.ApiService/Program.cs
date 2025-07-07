using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Tenants;
using TheBackendCmsSolution.Modules.Tenants.Services;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var modules = ModuleLoader.DiscoverModules().ToList();

builder.Services.AddSingleton<TenantServiceProviderFactory>(sp =>
    new TenantServiceProviderFactory(builder.Services, builder.Configuration, modules, sp));

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

// Apply host-level migrations for the tenants store
modules.OfType<TenancyModule>().FirstOrDefault()?.ApplyMigrations(app.Services);
modules.OfType<TheBackendCmsSolution.Modules.Identity.IdentityServerModule>().FirstOrDefault()?.ApplyMigrations(app.Services);

// Build per-tenant service providers
var providerFactory = app.Services.GetRequiredService<TenantServiceProviderFactory>();
providerFactory.InitializeAsync().GetAwaiter().GetResult();

app.UseMiddleware<TheBackendCmsSolution.Modules.Tenants.TenantResolutionMiddleware>();

foreach (var module in modules)
{
    module.MapRoutes(app);
}

app.MapDefaultEndpoints();
app.Run();

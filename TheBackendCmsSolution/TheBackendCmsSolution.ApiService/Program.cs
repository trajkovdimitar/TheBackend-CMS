using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Tenants;
using TheBackendCmsSolution.Modules.Tenants.Services;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var modules = ModuleLoader.DiscoverModules().ToList();

var baseServices = new ServiceCollection();
foreach (var sd in builder.Services)
{
    ((IServiceCollection)baseServices).Add(sd);
}

builder.Services.AddSingleton<TenantServiceProviderFactory>(sp =>
    new TenantServiceProviderFactory(baseServices, builder.Configuration, modules, sp));

foreach (var module in modules.Where(m => m is TenancyModule || m is TheBackendCmsSolution.Modules.Identity.IdentityServerModule))
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

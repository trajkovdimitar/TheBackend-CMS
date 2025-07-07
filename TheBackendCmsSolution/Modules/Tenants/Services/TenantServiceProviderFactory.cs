using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Tenants.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;
using TheBackendCmsSolution.Modules.Identity;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public class TenantServiceProviderFactory
{
    private readonly IServiceCollection _baseServices;
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ICmsModule> _modules;
    private readonly IServiceProvider _rootProvider;
    private readonly ConcurrentDictionary<string, IServiceProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public TenantServiceProviderFactory(IServiceCollection baseServices,
        IConfiguration configuration,
        IEnumerable<ICmsModule> modules,
        IServiceProvider rootProvider)
    {
        _baseServices = new ServiceCollection();
        foreach (var sd in baseServices.Where(d => d.ServiceType != typeof(TenantServiceProviderFactory)))
        {
            _baseServices.Add(sd);
        }
        _configuration = configuration;
        _modules = modules;
        _rootProvider = rootProvider;
    }

    public IReadOnlyDictionary<string, IServiceProvider> Providers => _providers;

    public async Task InitializeAsync()
    {
        using var scope = _rootProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var tenants = await db.Tenants.AsNoTracking().ToListAsync();
        foreach (var tenant in tenants)
        {
            var provider = BuildProviderForTenant(tenant);
            _providers[tenant.Name] = provider;
        }
    }

    private IServiceProvider BuildProviderForTenant(Tenant tenant)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        foreach (var sd in _baseServices)
        {
            ((IServiceCollection)services).Add(sd);
        }

        services.AddScoped<ITenantAccessor>(_ => new TenantAccessor { CurrentTenant = tenant });

        var enabled = tenant.EnabledModules ?? new List<string>();
        var selectedModules = _modules
            .Where(m => enabled.Contains(m.GetType().FullName))
            .Where(m => m is not TenancyModule && m is not TheBackendCmsSolution.Modules.Identity.OpenIdModule);
        foreach (var module in selectedModules)
        {
            module.ConfigureServices(services, _configuration);
        }

        var provider = services.BuildServiceProvider();

        foreach (var module in selectedModules)
        {
            module.ApplyMigrations(provider);
        }

        return provider;
    }

    public bool TryGetProvider(string tenantName, out IServiceProvider provider)
    {
        return _providers.TryGetValue(tenantName, out provider!);
    }
}

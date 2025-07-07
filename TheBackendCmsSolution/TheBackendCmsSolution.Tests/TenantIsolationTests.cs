using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TheBackendCmsSolution.Modules.Content;
using TheBackendCmsSolution.Modules.Content.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;
using TheBackendCmsSolution.Modules.Tenants.Services;
using Xunit;

namespace TheBackendCmsSolution.Tests;

public class TenantIsolationTests
{
    private static ServiceProvider BuildProvider(Tenant tenant)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITenantAccessor>(new TenantAccessor { CurrentTenant = tenant });

        var module = new ContentModule();
        var configuration = new ConfigurationBuilder().Build();
        module.ConfigureServices(services, configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void ServiceProviders_Have_Distinct_ConnectionStrings()
    {
        var tenant1 = new Tenant { Name = "t1", ConnectionString = "Host=db;Database=db1" };
        var tenant2 = new Tenant { Name = "t2", ConnectionString = "Host=db;Database=db2" };

        using var provider1 = BuildProvider(tenant1);
        using var provider2 = BuildProvider(tenant2);

        using var scope1 = provider1.CreateScope();
        using var scope2 = provider2.CreateScope();

        var db1 = scope1.ServiceProvider.GetRequiredService<ContentDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<ContentDbContext>();

        Assert.Contains("db1", db1.Database.GetDbConnection().ConnectionString);
        Assert.Contains("db2", db2.Database.GetDbConnection().ConnectionString);
        Assert.NotEqual(db1.Database.GetDbConnection().ConnectionString, db2.Database.GetDbConnection().ConnectionString);
    }
}

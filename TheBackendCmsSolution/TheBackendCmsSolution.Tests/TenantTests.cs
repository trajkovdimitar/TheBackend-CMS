using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TheBackendCmsSolution.Modules.Tenants.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;
using TheBackendCmsSolution.Modules.Tenants.Services;
using TheBackendCmsSolution.Modules.Tenants;
using TheBackendCmsSolution.Modules.Content;
using TheBackendCmsSolution.Modules.Content.Data;

namespace TheBackendCmsSolution.Tests;

public class TenantTests
{
    private static TenantDbContext CreateTenantDbContext()
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase("tenants")
            .Options;
        return new TenantDbContext(options);
    }

    [Fact]
    public async Task TenantResolver_Resolves_ByHost()
    {
        using var db = CreateTenantDbContext();
        db.Tenants.AddRange(
            new Tenant { Id = Guid.NewGuid(), Name = "t1", Host = "t1.example.com", ConnectionString = "cs1" },
            new Tenant { Id = Guid.NewGuid(), Name = "t2", Host = "t2.example.com", ConnectionString = "cs2" });
        await db.SaveChangesAsync();

        var resolver = new TenantResolver(db);
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("t2.example.com");

        var tenant = await resolver.ResolveAsync(context);
        Assert.NotNull(tenant);
        Assert.Equal("t2", tenant!.Name);
    }


    [Fact]
    public async Task Middleware_Sets_Tenant_On_HttpContext_And_Accessor()
    {
        using var db = CreateTenantDbContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "t", Host = "tenant.com", ConnectionString = "cs" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var resolver = new TenantResolver(db);
        var accessor = new TenantAccessor();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tenant.com");

        await middleware.InvokeAsync(context, resolver, accessor);

        Assert.NotNull(accessor.CurrentTenant);
        Assert.Equal(tenant.Id, accessor.CurrentTenant!.Id);
        Assert.Same(accessor.CurrentTenant, context.Items["Tenant"] as Tenant);
    }

    [Fact]
    public void ContentModule_Uses_Tenant_ConnectionString()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var accessor = new TenantAccessor
        {
            CurrentTenant = new Tenant { ConnectionString = "Host=mydb;Database=db1" }
        };
        services.AddSingleton<ITenantAccessor>(accessor);

        var module = new ContentModule();
        module.ConfigureServices(services, configuration);

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        Assert.Contains("Host=mydb;Database=db1", db.Database.GetDbConnection().ConnectionString);
    }
}

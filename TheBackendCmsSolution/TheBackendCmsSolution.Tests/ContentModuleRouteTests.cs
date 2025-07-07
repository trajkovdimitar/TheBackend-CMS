using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheBackendCmsSolution.Modules.Content;
using TheBackendCmsSolution.Modules.Content.Data;
using TheBackendCmsSolution.Modules.Content.Dtos;
using TheBackendCmsSolution.Modules.Content.Models;
using TheBackendCmsSolution.Modules.Tenants.Services;
using TheBackendCmsSolution.Modules.Tenants.Models;
using Xunit;

public class ContentModuleRouteTests
{
    private static HttpClient CreateClient()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddDbContext<ContentDbContext>(o => o.UseInMemoryDatabase("content"));
        builder.Services.AddSingleton<ITenantAccessor>(new TenantAccessor
        {
            CurrentTenant = new Tenant { ConnectionString = "Host=memory;Database=test" }
        });
        var app = builder.Build();
        var module = new ContentModule();
        module.MapRoutes(app);
        app.StartAsync().GetAwaiter().GetResult();
        return app.GetTestClient();
    }

    [Fact]
    public async Task CreateAndRetrieveContentType()
    {
        using var client = CreateClient();
        var dto = new ContentTypeDto("article", "Article", new());
        var create = await client.PostAsJsonAsync("/content-types", dto);
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<ContentType>();

        var get = await client.GetAsync($"/content-types/{created!.Id}");
        get.EnsureSuccessStatusCode();
        var retrieved = await get.Content.ReadFromJsonAsync<ContentType>();

        Assert.Equal(dto.Name, retrieved!.Name);
    }
}

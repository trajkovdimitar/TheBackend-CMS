using Microsoft.AspNetCore.Http;
using TheBackendCmsSolution.Modules.Tenants.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TheBackendCmsSolution.Modules.Tenants;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
                                  ITenantResolver resolver,
                                  ITenantAccessor accessor,
                                  TenantServiceProviderFactory providerFactory)
    {
        var tenant = await resolver.ResolveAsync(context);
        accessor.CurrentTenant = tenant;
        context.Items["Tenant"] = tenant;
        if (tenant != null && providerFactory.TryGetProvider(tenant.Name, out var provider))
        {
            using var scope = provider.CreateScope();
            context.RequestServices = scope.ServiceProvider;
            await _next(context);
        }
        else
        {
            await _next(context);
        }
    }
}

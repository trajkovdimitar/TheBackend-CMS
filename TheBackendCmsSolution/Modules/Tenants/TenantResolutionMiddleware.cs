using TheBackendCmsSolution.Modules.Tenants.Services;

namespace TheBackendCmsSolution.Modules.Tenants;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver, ITenantAccessor accessor)
    {
        var tenant = await resolver.ResolveAsync(context);
        accessor.CurrentTenant = tenant;
        context.Items["Tenant"] = tenant;
        await _next(context);
    }
}

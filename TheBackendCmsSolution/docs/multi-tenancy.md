# Multi-tenancy Design

This document outlines how to extend **TheBackend-CMS** with tenant isolation similar to Orchard Core.

## Goals
- Isolate content, settings and enabled modules per tenant.
- Resolve the current tenant for each request.
- Allow different connection strings or database schemas for tenants.

## Database
1. **Tenants table** (in a shared configuration database)
   - `Id` (GUID)
   - `Name` (unique)
   - `Host` or `UrlPrefix` used to match requests
   - `ConnectionString` for the tenant's data store
   - Any additional settings (e.g., theme, enabled modules)

2. **Per-tenant databases**
   - Each tenant uses the same EF Core migrations for modules but with a separate connection string.
   - Optionally use schemas instead of separate databases (`tenant1.ContentItems`, `tenant2.ContentItems`, etc.).

3. **Module migrations**
   - When a tenant is created, run migrations for each enabled module against the tenant database.
   - Modules should reference `IConfiguration` to retrieve the current tenant's connection string.

## Request Pipeline
1. **Tenant resolution middleware**
   - Runs early in the pipeline.
   - Looks at the host name or URL prefix to find the tenant record in the Tenants table.
   - Stores the resolved tenant in `HttpContext.Items` for later access.

2. **Per-tenant service provider**
   - Build a `IServiceProvider` for each tenant on startup (similar to Orchard Core shells).
   - Store providers in a dictionary keyed by tenant name.
   - After tenant resolution, create a scope from the tenant's provider and set it as the request `IServiceProvider`.

3. **Module activation**
   - Modules implement `ICmsModule` as today but can be enabled/disabled per tenant.
   - During tenant service provider creation, only add services from modules listed in the tenant configuration.

4. **Data access**
   - DbContext factories or connection string resolvers use the current tenant info from `HttpContext` to connect to the correct database.

## Example Middleware Skeleton
```csharp
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver)
    {
        var tenant = await resolver.ResolveAsync(context);
        context.Items["Tenant"] = tenant;
        using var scope = tenant.ServiceProvider.CreateScope();
        context.RequestServices = scope.ServiceProvider;
        await _next(context);
    }
}
```

## Next Steps
- Implement `ITenantResolver` and a tenant store for managing tenant records.
- Add endpoints to create and manage tenants.
- Update modules to read connection strings from the resolved tenant.
- Write integration tests for tenant isolation.

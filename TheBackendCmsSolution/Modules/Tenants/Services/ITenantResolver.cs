using Microsoft.AspNetCore.Http;
using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public interface ITenantResolver
{
    Task<Tenant?> ResolveAsync(HttpContext context);
}

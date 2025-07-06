using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Tenants.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public class TenantResolver : ITenantResolver
{
    private readonly TenantDbContext _db;

    public TenantResolver(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> ResolveAsync(HttpContext context)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();
        var path = context.Request.Path.Value ?? string.Empty;
        return await _db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Host == host ||
                                    (t.UrlPrefix != null && path.StartsWith($"/{t.UrlPrefix}", StringComparison.OrdinalIgnoreCase)));
    }
}

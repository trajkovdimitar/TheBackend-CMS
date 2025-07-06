using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Tenants.Data;
using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public class TenantStore : ITenantStore
{
    private readonly TenantDbContext _db;

    public TenantStore(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<Tenant>> GetAllAsync() => await _db.Tenants.AsNoTracking().ToListAsync();

    public async Task<Tenant?> GetAsync(Guid id) => await _db.Tenants.FindAsync(id);

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        tenant.Id = Guid.NewGuid();
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        return tenant;
    }

    public async Task<bool> UpdateAsync(Guid id, Tenant tenant)
    {
        var existing = await _db.Tenants.FindAsync(id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = tenant.Name;
        existing.Host = tenant.Host;
        existing.UrlPrefix = tenant.UrlPrefix;
        existing.ConnectionString = tenant.ConnectionString;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _db.Tenants.FindAsync(id);
        if (existing is null)
        {
            return false;
        }
        _db.Tenants.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}

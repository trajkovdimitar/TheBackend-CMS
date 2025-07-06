using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public interface ITenantStore
{
    Task<List<Tenant>> GetAllAsync();
    Task<Tenant?> GetAsync(Guid id);
    Task<Tenant> CreateAsync(Tenant tenant);
    Task<bool> UpdateAsync(Guid id, Tenant tenant);
    Task<bool> DeleteAsync(Guid id);
}

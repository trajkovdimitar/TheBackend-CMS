using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public interface ITenantAccessor
{
    Tenant? CurrentTenant { get; set; }
}

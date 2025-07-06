using TheBackendCmsSolution.Modules.Tenants.Models;

namespace TheBackendCmsSolution.Modules.Tenants.Services;

public class TenantAccessor : ITenantAccessor
{
    public Tenant? CurrentTenant { get; set; }
}

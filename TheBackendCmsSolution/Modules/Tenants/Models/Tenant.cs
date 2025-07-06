namespace TheBackendCmsSolution.Modules.Tenants.Models;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Host { get; set; }
    public string? UrlPrefix { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}

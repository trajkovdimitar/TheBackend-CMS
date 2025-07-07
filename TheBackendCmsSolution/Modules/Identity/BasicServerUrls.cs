using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Http;

namespace TheBackendCmsSolution.Modules.Identity;

public class BasicServerUrls : IServerUrls
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BasicServerUrls(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Origin { get; set; } = string.Empty;
    public string BasePath { get; set; } = string.Empty;
    public string BaseUrl => (string.IsNullOrEmpty(Origin) ? GetOrigin() : Origin) + BasePath;

    private string GetOrigin()
    {
        var req = _httpContextAccessor.HttpContext?.Request;
        if (req == null) return string.Empty;
        return $"{req.Scheme}://{req.Host}";
    }
}

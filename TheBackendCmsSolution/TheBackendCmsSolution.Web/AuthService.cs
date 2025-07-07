using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TheBackendCmsSolution.Web;

public class AuthService
{
    private readonly HttpClient _http;
    public string? AccessToken { get; private set; }
    public event Action? AuthenticationStateChanged;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string,string>("grant_type","password"),
            new KeyValuePair<string,string>("username",username),
            new KeyValuePair<string,string>("password",password),
            new KeyValuePair<string,string>("scope","cms.api")
        ]);
        var response = await _http.PostAsync("/connect/token", content);
        if (!response.IsSuccessStatusCode)
            return false;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        AccessToken = json.GetProperty("access_token").GetString();
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        AuthenticationStateChanged?.Invoke();
        return true;
    }

    public Task LogoutAsync()
    {
        AccessToken = null;
        _http.DefaultRequestHeaders.Authorization = null;
        AuthenticationStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public ClaimsPrincipal GetPrincipal()
    {
        if (string.IsNullOrEmpty(AccessToken))
            return new ClaimsPrincipal(new ClaimsIdentity());
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(AccessToken);
        return new ClaimsPrincipal(new ClaimsIdentity(token.Claims, "Bearer"));
    }
}

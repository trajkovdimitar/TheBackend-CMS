using Microsoft.AspNetCore.Components.Authorization;

namespace TheBackendCmsSolution.Web;

public class JwtAuthenticationStateProvider(AuthService auth) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = auth.GetPrincipal();
        return Task.FromResult(new AuthenticationState(principal));
    }

    internal void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

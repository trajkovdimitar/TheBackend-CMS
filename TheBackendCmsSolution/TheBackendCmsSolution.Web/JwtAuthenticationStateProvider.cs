using Microsoft.AspNetCore.Components.Authorization;

namespace TheBackendCmsSolution.Web;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _auth;

    public JwtAuthenticationStateProvider(AuthService auth)
    {
        _auth = auth;
        _auth.AuthenticationStateChanged += NotifyAuthenticationStateChangedInternal;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = _auth.GetPrincipal();
        return Task.FromResult(new AuthenticationState(principal));
    }

    private void NotifyAuthenticationStateChangedInternal()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

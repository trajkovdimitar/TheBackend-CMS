using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.OpenId.Data;
using TheBackendCmsSolution.Modules.OpenId.Models;

namespace TheBackendCmsSolution.Modules.OpenId;

public class OpenIdModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("authdb") ??
                "Host=localhost;Port=5434;Database=authdb;Username=postgres;Password=postgres";
            options.UseNpgsql(connectionString);
            options.UseOpenIddict();
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.AllowPasswordFlow();
                options.AcceptAnonymousClients();
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();
                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth");
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
    }

    private static async Task<IResult> RegisterAsync(AppUser user, AuthDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.UserName == user.UserName))
        {
            return Results.BadRequest(new { error = "User exists" });
        }
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> LoginAsync(HttpContext httpContext, AuthDbContext db)
    {
        var form = await httpContext.Request.ReadFormAsync();
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return Results.BadRequest(new { error = "Invalid credentials" });
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
            new(OpenIddictConstants.Claims.Username, user.UserName)
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(identity));
        return Results.Ok();
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.Migrate();
    }
}

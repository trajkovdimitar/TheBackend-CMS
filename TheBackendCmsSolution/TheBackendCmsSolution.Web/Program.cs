using TheBackendCmsSolution.Web;
using TheBackendCmsSolution.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient("api", client =>
    client.BaseAddress = new("https+http://apiservice"));
builder.Services.AddScoped<AuthService>(sp =>
    new AuthService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"),
        sp.GetRequiredService<JwtAuthenticationStateProvider>()));
builder.Services.AddScoped<ContentApiClient>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("api");
    return new ContentApiClient(client);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

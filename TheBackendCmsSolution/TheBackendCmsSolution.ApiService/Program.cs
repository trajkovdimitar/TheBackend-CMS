using TheBackendCmsSolution.Modules.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var modules = ModuleLoader.DiscoverModules();

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

foreach (var module in modules)
{
    module.ApplyMigrations(app.Services);
    module.MapRoutes(app);
}

app.MapDefaultEndpoints();
app.Run();

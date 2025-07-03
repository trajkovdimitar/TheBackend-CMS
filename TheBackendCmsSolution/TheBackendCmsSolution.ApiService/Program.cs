using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.ApiService.Data;
using TheBackendCmsSolution.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add EF Core with PostgreSQL
builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("contentdb") ??
                      "Host=localhost;Port=5433;Database=contentdb;Username=postgres;Password=postgres"));

var app = builder.Build();

// Delay migration until database is available
async Task EnsureDatabaseMigratedAsync(IServiceProvider serviceProvider)
{
    var maxRetries = 10;
    var retryDelay = TimeSpan.FromSeconds(2);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
            dbContext.Database.Migrate();
            Console.WriteLine("Database migration completed successfully.");
            break;
        }
        catch (Npgsql.NpgsqlException ex)
        {
            Console.WriteLine($"Migration attempt {i + 1}/{maxRetries} failed: {ex.Message}");
            if (i == maxRetries - 1) throw;
            await Task.Delay(retryDelay);
        }
    }
}

// Run migration asynchronously before starting the app
await EnsureDatabaseMigratedAsync(app.Services);

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello from TheBackend-CMS!");

// API Endpoints
app.MapPost("/content", async (CmsDbContext db, ContentItem item) =>
{
    item.Id = Guid.NewGuid();
    item.CreatedAt = DateTime.UtcNow;
    db.ContentItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/content/{item.Id}", item);
});

app.MapGet("/content/{id:guid}", async (CmsDbContext db, Guid id) =>
{
    var item = await db.ContentItems.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapGet("/content", async (CmsDbContext db) =>
{
    var items = await db.ContentItems.ToListAsync();
    return Results.Ok(items);
});

app.Run();
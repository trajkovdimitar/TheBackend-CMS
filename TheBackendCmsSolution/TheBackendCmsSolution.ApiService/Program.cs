using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.ApiService.Data;
using TheBackendCmsSolution.ApiService.Models;
using Microsoft.AspNetCore.Mvc; // For ControllerBase

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add EF Core with PostgreSQL
builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("contentdb") ??
                      "Host=localhost;Port=5433;Database=contentdb;Username=postgres;Password=postgres"));

// Configure JSON options to handle cycles (though we use projections)
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 64; // Default, can adjust if needed
});

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

// Seed ContentTypes if not exists
async Task SeedContentTypesAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CmsDbContext>();

    if (!dbContext.ContentTypes.Any())
    {
        dbContext.ContentTypes.AddRange(
            new ContentType { Name = "blogpost", DisplayName = "Blog Post" },
            new ContentType { Name = "page", DisplayName = "Page" }
        );
        await dbContext.SaveChangesAsync();
        Console.WriteLine("ContentTypes seeded successfully.");
    }
}

// Run migration and seeding asynchronously before starting the app
await EnsureDatabaseMigratedAsync(app.Services);
await SeedContentTypesAsync(app.Services);

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
    return Results.Created($"/api/content/{item.Type}/{item.Id}", item);
});

app.MapGet("/content/{id:guid}", async (Guid id, CmsDbContext db) =>
{
    var item = await db.ContentItems
        .Where(i => i.Id == id)
        .Select(i => new
        {
            i.Id,
            i.Title,
            i.Body,
            i.Type,
            i.CreatedAt,
            i.UpdatedAt,
            ContentType = new { i.ContentType.Name, i.ContentType.DisplayName }
        })
        .FirstOrDefaultAsync();
    return item != null ? Results.Ok(item) : Results.NotFound();
});

app.MapGet("/content", async (CmsDbContext db) =>
{
    var items = await db.ContentItems
        .Select(i => new
        {
            i.Id,
            i.Title,
            i.Body,
            i.Type,
            i.CreatedAt,
            i.UpdatedAt,
            ContentType = new { i.ContentType.Name, i.ContentType.DisplayName }
        })
        .ToListAsync();
    return Results.Ok(items);
});

app.MapGet("/api/content/{type}", async (string type, CmsDbContext db) =>
{
    var contentType = await db.ContentTypes.FindAsync(type);
    if (contentType == null)
    {
        return Results.NotFound($"Content type '{type}' not found.");
    }
    var items = await db.ContentItems
        .Where(i => i.Type == type)
        .Select(i => new
        {
            i.Id,
            i.Title,
            i.Body,
            i.Type,
            i.CreatedAt,
            i.UpdatedAt,
            ContentType = new { i.ContentType.Name, i.ContentType.DisplayName }
        })
        .ToListAsync();
    return Results.Ok(items);
});

app.MapPut("/content/{id:guid}", async (Guid id, CmsDbContext db, ContentItem item) =>
{
    var existingItem = await db.ContentItems.FindAsync(id);
    if (existingItem == null) return Results.NotFound();

    existingItem.Title = item.Title;
    existingItem.Body = item.Body;
    existingItem.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        existingItem.Id,
        existingItem.Title,
        existingItem.Body,
        existingItem.Type,
        existingItem.CreatedAt,
        existingItem.UpdatedAt,
        ContentType = new { existingItem.ContentType.Name, existingItem.ContentType.DisplayName }
    });
});

app.MapDelete("/content/{id:guid}", async (Guid id, CmsDbContext db) =>
{
    var item = await db.ContentItems.FindAsync(id);
    if (item == null) return Results.NotFound();

    db.ContentItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
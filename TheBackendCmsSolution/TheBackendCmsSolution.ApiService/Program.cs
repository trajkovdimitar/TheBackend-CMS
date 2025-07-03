using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.ApiService.Data;
using TheBackendCmsSolution.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("contentdb") ??
                      "Host=localhost;Port=5433;Database=contentdb;Username=postgres;Password=postgres"));

var app = builder.Build();

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

await EnsureDatabaseMigratedAsync(app.Services);

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello from TheBackend-CMS!");

app.MapPost("/content", async (CmsDbContext db, ContentItem item) =>
{
    item.Id = Guid.NewGuid();
    item.CreatedAt = DateTime.UtcNow;
    db.ContentItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/content/{item.Type}/{item.Id}", item);
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

app.MapGet("/content/{type}", async (string type, CmsDbContext db) =>
{
    var items = await db.ContentItems.Where(i => i.Type == type).ToListAsync();
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
    return Results.Ok(existingItem);
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
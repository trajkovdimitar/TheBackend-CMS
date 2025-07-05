using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Npgsql;
using TheBackendCmsSolution.ApiService.Data;
using TheBackendCmsSolution.ApiService.Dtos;
using TheBackendCmsSolution.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add EF Core with PostgreSQL and enable dynamic JSON
var connectionString = builder.Configuration.GetConnectionString("contentdb") ??
                       "Host=localhost;Port=5433;Database=contentdb;Username=postgres;Password=postgres";
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseNpgsql(dataSourceBuilder.Build()));

// Add logging
builder.Services.AddLogging();

// Configure JSON options (no reference metadata)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = null;
    options.SerializerOptions.MaxDepth = 64;
});

var app = builder.Build();

// Validation helper method
static ValidationProblemDetails ValidateDto(object dto, ILogger logger)
{
    var validationContext = new ValidationContext(dto);
    var validationResults = new List<ValidationResult>();
    bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

    if (!isValid)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var result in validationResults)
        {
            foreach (var memberName in result.MemberNames)
            {
                if (!errors.ContainsKey(memberName))
                {
                    errors[memberName] = new[] { result.ErrorMessage ?? "Invalid value" };
                }
            }
        }
        logger.LogWarning("Validation failed for DTO: {Errors}", errors);
        return new ValidationProblemDetails(errors)
        {
            Status = 400,
            Title = "Validation Failed"
        };
    }
    return null;
}

// Migration with retry logic
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
            await dbContext.Database.MigrateAsync();
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

// Seed ContentTypes
async Task SeedContentTypesAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CmsDbContext>();

    if (!await dbContext.ContentTypes.AnyAsync())
    {
        dbContext.ContentTypes.AddRange(
            new ContentType
            {
                Id = Guid.NewGuid(),
                Name = "blogpost",
                DisplayName = "Blog Post",
                Fields = new Dictionary<string, object>
                {
                    { "Body", "string" },
                    { "Tags", "string[]" }
                },
                CreatedAt = DateTime.UtcNow
            },
            new ContentType
            {
                Id = Guid.NewGuid(),
                Name = "page",
                DisplayName = "Page",
                Fields = new Dictionary<string, object>
                {
                    { "Content", "string" }
                },
                CreatedAt = DateTime.UtcNow
            }
        );
        await dbContext.SaveChangesAsync();
        Console.WriteLine("ContentTypes seeded successfully.");
    }
}

// Run migration and seeding before starting the app
await EnsureDatabaseMigratedAsync(app.Services);
await SeedContentTypesAsync(app.Services);

// Content Type Endpoints
app.MapPost("/content-types", async (ContentTypeDto dto, CmsDbContext db, ILogger<Program> logger) =>
{
    var validationErrors = ValidateDto(dto, logger);
    if (validationErrors != null)
    {
        return Results.BadRequest(validationErrors);
    }

    var contentType = new ContentType
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        DisplayName = dto.DisplayName,
        Fields = dto.Fields,
        CreatedAt = DateTime.UtcNow
    };

    db.ContentTypes.Add(contentType);
    await db.SaveChangesAsync();
    logger.LogInformation("Created content type {Id}", contentType.Id);
    return Results.Created($"/content-types/{contentType.Id}", contentType);
});

app.MapGet("/content-types", async (CmsDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Retrieving all content types");
    return await db.ContentTypes.ToListAsync();
});

// Content Item Endpoints
app.MapGet("/content", async (CmsDbContext db, ILogger<Program> logger) =>
{
    logger.LogInformation("Retrieving all content items");
    var items = await db.ContentItems
        .Include(c => c.ContentType)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.ContentTypeId,
            ContentType = new { c.ContentType.Name, c.ContentType.DisplayName },
            c.Fields,
            c.CreatedAt,
            c.UpdatedAt
        })
        .ToListAsync();
    return Results.Ok(items);
});

app.MapGet("/content/{id:guid}", async (Guid id, CmsDbContext db, ILogger<Program> logger) =>
{
    var item = await db.ContentItems
        .Include(c => c.ContentType)
        .Where(c => c.Id == id)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.ContentTypeId,
            ContentType = new { c.ContentType.Name, c.ContentType.DisplayName },
            c.Fields,
            c.CreatedAt,
            c.UpdatedAt
        })
        .FirstOrDefaultAsync();
    if (item == null)
    {
        logger.LogWarning("Content item {Id} not found", id);
        return Results.NotFound();
    }
    return Results.Ok(item);
});

app.MapGet("/api/content/{type}", async (string type, CmsDbContext db, ILogger<Program> logger) =>
{
    var contentType = await db.ContentTypes.FirstOrDefaultAsync(ct => ct.Name == type);
    if (contentType == null)
    {
        logger.LogWarning("Content type {Type} not found", type);
        return Results.NotFound();
    }
    var items = await db.ContentItems
        .Where(c => c.ContentTypeId == contentType.Id)
        .Include(c => c.ContentType)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.ContentTypeId,
            ContentType = new { c.ContentType.Name, c.ContentType.DisplayName },
            c.Fields,
            c.CreatedAt,
            c.UpdatedAt
        })
        .ToListAsync();
    return Results.Ok(items);
});

app.MapPost("/content", async (ContentItemDto dto, CmsDbContext db, ILogger<Program> logger) =>
{
    var validationErrors = ValidateDto(dto, logger);
    if (validationErrors != null)
    {
        return Results.BadRequest(validationErrors);
    }

    var contentType = await db.ContentTypes.FindAsync(dto.ContentTypeId);
    if (contentType == null)
    {
        logger.LogWarning("Content type {Id} not found", dto.ContentTypeId);
        return Results.BadRequest(new { Error = "Invalid ContentTypeId" });
    }

    var item = new ContentItem
    {
        Id = Guid.NewGuid(),
        Title = dto.Title,
        ContentTypeId = dto.ContentTypeId,
        Fields = dto.Fields,
        CreatedAt = DateTime.UtcNow
    };

    db.ContentItems.Add(item);
    await db.SaveChangesAsync();
    logger.LogInformation("Created content item {Id}", item.Id);
    return Results.Created($"/content/{item.Id}", item);
});

app.MapPut("/content/{id:guid}", async (Guid id, ContentItemDto dto, CmsDbContext db, ILogger<Program> logger) =>
{
    var validationErrors = ValidateDto(dto, logger);
    if (validationErrors != null)
    {
        return Results.BadRequest(validationErrors);
    }

    var item = await db.ContentItems.FindAsync(id);
    if (item == null)
    {
        logger.LogWarning("Content item {Id} not found for update", id);
        return Results.NotFound();
    }

    var contentType = await db.ContentTypes.FindAsync(dto.ContentTypeId);
    if (contentType == null)
    {
        logger.LogWarning("Content type {Id} not found", dto.ContentTypeId);
        return Results.BadRequest(new { Error = "Invalid ContentTypeId" });
    }

    item.Title = dto.Title;
    item.ContentTypeId = dto.ContentTypeId;
    item.Fields = dto.Fields;
    item.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    logger.LogInformation("Updated content item {Id}", id);
    return Results.Ok(item);
});

app.MapDelete("/content/{id:guid}", async (Guid id, CmsDbContext db, ILogger<Program> logger) =>
{
    var item = await db.ContentItems.FindAsync(id);
    if (item == null)
    {
        logger.LogWarning("Content item {Id} not found for deletion", id);
        return Results.NotFound();
    }

    db.ContentItems.Remove(item);
    await db.SaveChangesAsync();
    logger.LogInformation("Deleted content item {Id}", id);
    return Results.NoContent();
});

app.Run();
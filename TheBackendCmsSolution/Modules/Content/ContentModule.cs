using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Content.Data;
using TheBackendCmsSolution.Modules.Content.Dtos;
using TheBackendCmsSolution.Modules.Content.Models;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TheBackendCmsSolution.Modules.Tenants.Services;

namespace TheBackendCmsSolution.Modules.Content;

public class ContentModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ContentDbContext>((sp, options) =>
        {
            var accessor = sp.GetRequiredService<ITenantAccessor>();
            var connectionString = accessor.CurrentTenant?.ConnectionString ??
                                   config.GetConnectionString("contentdb") ??
                                   "Host=localhost;Port=5432;Database=contentdb;Username=postgres;Password=postgres";
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            options.UseNpgsql(dataSourceBuilder.Build());
        });
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapPost("/content-types", async (ContentTypeDto dto, ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapGet("/content-types", async (ContentDbContext db, ILogger<ContentModule> logger) =>
        {
            logger.LogInformation("Retrieving all content types");
            return await db.ContentTypes.ToListAsync();
        });

        group.MapGet("/content-types/{id:guid}", async (Guid id, ContentDbContext db, ILogger<ContentModule> logger) =>
        {
            var ct = await db.ContentTypes.FindAsync(id);
            if (ct is null)
            {
                logger.LogWarning("Content type {Id} not found", id);
                return Results.NotFound();
            }
            return Results.Ok(ct);
        });

        group.MapPut("/content-types/{id:guid}", async (Guid id, ContentTypeDto dto, ContentDbContext db, ILogger<ContentModule> logger) =>
        {
            var validationErrors = ValidateDto(dto, logger);
            if (validationErrors != null)
            {
                return Results.BadRequest(validationErrors);
            }

            var existing = await db.ContentTypes.FindAsync(id);
            if (existing is null)
            {
                logger.LogWarning("Content type {Id} not found for update", id);
                return Results.NotFound();
            }

            existing.Name = dto.Name;
            existing.DisplayName = dto.DisplayName;
            existing.Fields = dto.Fields;

            await db.SaveChangesAsync();
            logger.LogInformation("Updated content type {Id}", id);
            return Results.Ok(existing);
        });

        group.MapDelete("/content-types/{id:guid}", async (Guid id, ContentDbContext db, ILogger<ContentModule> logger) =>
        {
            var ct = await db.ContentTypes.FindAsync(id);
            if (ct is null)
            {
                logger.LogWarning("Content type {Id} not found for deletion", id);
                return Results.NotFound();
            }
            db.ContentTypes.Remove(ct);
            await db.SaveChangesAsync();
            logger.LogInformation("Deleted content type {Id}", id);
            return Results.NoContent();
        });

        group.MapGet("/content", async (ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapGet("/content/{id:guid}", async (Guid id, ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapGet("/api/content/{type}", async (string type, ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapPost("/content", async (ContentItemDto dto, ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapPut("/content/{id:guid}", async (Guid id, ContentItemDto dto, ContentDbContext db, ILogger<ContentModule> logger) =>
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

        group.MapDelete("/content/{id:guid}", async (Guid id, ContentDbContext db, ILogger<ContentModule> logger) =>
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
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        db.Database.Migrate();
        SeedContentTypesAsync(db).GetAwaiter().GetResult();
    }

    private static async Task SeedContentTypesAsync(ContentDbContext db)
    {
        if (!await db.ContentTypes.AnyAsync())
        {
            db.ContentTypes.AddRange(
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
            await db.SaveChangesAsync();
        }
    }

    private static ValidationProblemDetails? ValidateDto(object dto, ILogger logger)
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
}

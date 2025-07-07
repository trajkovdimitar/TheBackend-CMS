using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheBackendCmsSolution.Modules.Abstractions;
using TheBackendCmsSolution.Modules.Users.Data;
using TheBackendCmsSolution.Modules.Users.Models;

namespace TheBackendCmsSolution.Modules.Users;

public class UsersModule : ICmsModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("usersdb") ??
                              "Host=localhost;Port=5435;Database=usersdb;Username=postgres;Password=postgres";
            var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connection);
            dataSourceBuilder.EnableDynamicJson();
            options.UseNpgsql(dataSourceBuilder.Build());
        });
    }

    public void MapRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/users");

        group.MapGet("/", async (UsersDbContext db) => await db.Users.ToListAsync());

        group.MapGet("/{id:guid}", async (Guid id, UsersDbContext db, ILogger<UsersModule> logger) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
            {
                logger.LogWarning("User {Id} not found", id);
                return Results.NotFound();
            }
            return Results.Ok(user);
        });

        group.MapPost("/", async (User user, UsersDbContext db, ILogger<UsersModule> logger) =>
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            db.Users.Add(user);
            await db.SaveChangesAsync();
            logger.LogInformation("Created user {Id}", user.Id);
            return Results.Created($"/users/{user.Id}", user);
        });

        group.MapPut("/{id:guid}", async (Guid id, User user, UsersDbContext db, ILogger<UsersModule> logger) =>
        {
            var existing = await db.Users.FindAsync(id);
            if (existing is null)
            {
                logger.LogWarning("User {Id} not found for update", id);
                return Results.NotFound();
            }
            existing.UserName = user.UserName;
            existing.Email = user.Email;
            existing.PasswordHash = user.PasswordHash;
            await db.SaveChangesAsync();
            logger.LogInformation("Updated user {Id}", id);
            return Results.Ok(existing);
        });

        group.MapDelete("/{id:guid}", async (Guid id, UsersDbContext db, ILogger<UsersModule> logger) =>
        {
            var existing = await db.Users.FindAsync(id);
            if (existing is null)
            {
                logger.LogWarning("User {Id} not found for deletion", id);
                return Results.NotFound();
            }
            db.Users.Remove(existing);
            await db.SaveChangesAsync();
            logger.LogInformation("Deleted user {Id}", id);
            return Results.NoContent();
        });
    }

    public void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        db.Database.Migrate();

        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}

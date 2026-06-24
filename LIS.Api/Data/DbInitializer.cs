using LIS.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LIS.Api.Data;

/// <summary>
/// Applies pending EF Core migrations (with a retry loop so the API can start before the
/// SQL Server container finishes accepting connections) and seeds the initial users.
/// </summary>
public static class DbInitializer
{
    private const int MaxAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public static async Task MigrateAndSeedAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LisDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied successfully.");
                break;
            }
            catch (SqlException) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    "SQL Server not ready (attempt {Attempt}/{Max}); retrying in {Delay}s...",
                    attempt, MaxAttempts, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        await SeedUsersAsync(db, hasher, logger, cancellationToken);
    }

    private static async Task SeedUsersAsync(
        LisDbContext db,
        IPasswordHasher<User> hasher,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var seedUsers = new[]
        {
            CreateUser("admin", "Admin123!", UserRole.Admin, hasher),
            CreateUser("tech", "Tech123!", UserRole.Technician, hasher)
        };

        db.Users.AddRange(seedUsers);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} users (admin, tech).", seedUsers.Length);
    }

    private static User CreateUser(string username, string password, UserRole role, IPasswordHasher<User> hasher)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Role = role
        };
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}

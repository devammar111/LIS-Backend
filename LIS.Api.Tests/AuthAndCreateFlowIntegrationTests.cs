using System.Net;
using System.Net.Http.Json;
using LIS.Api.Data;
using LIS.Api.Data.Interceptors;
using LIS.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LIS.Api.Tests;

public class AuthAndCreateFlowIntegrationTests : IClassFixture<LisApiFactory>
{
    private readonly LisApiFactory _factory;

    public AuthAndCreateFlowIntegrationTests(LisApiFactory factory)
    {
        _factory = factory;
    }

    private record TokenPayload(string Token, string Username, string Role, DateTime ExpiresAtUtc);

    [Fact]
    public async Task FullFlow_Login_Create_List_Works_And_Unauthenticated_Is401()
    {
        var client = _factory.CreateClient();

        // Unauthenticated create -> 401
        var unauth = await client.PostAsJsonAsync("/api/orders", new
        {
            patientName = "No Auth",
            testType = "CBC",
            priority = "Routine",
            collectionDate = DateTime.Today.ToString("yyyy-MM-dd")
        });
        Assert.Equal(HttpStatusCode.Unauthorized, unauth.StatusCode);

        // Login as the seeded technician
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "tech", password = "Tech123!" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<TokenPayload>())!.Token;
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Authenticated create -> 201
        var create = await client.PostAsJsonAsync("/api/orders", new
        {
            patientName = "Jane Doe",
            testType = "Lipid Panel",
            priority = "STAT",
            collectionDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        // List -> paged envelope with the created order
        var list = await client.GetFromJsonAsync<PagedResponse<OrderResponse>>("/api/orders?page=1&pageSize=10");
        Assert.NotNull(list);
        Assert.True(list!.TotalCount >= 1);
        Assert.Contains(list.Items, o => o.PatientName == "Jane Doe" && o.TestType == "Lipid Panel");
    }

    [Fact]
    public async Task InvalidBody_Returns400_WithFieldErrors()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "tech", password = "Tech123!" });
        var token = (await login.Content.ReadFromJsonAsync<TokenPayload>())!.Token;
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            patientName = "",
            testType = "Invalid",
            priority = "Urgent",
            collectionDate = "2020-01-01"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("patientName", body);
        Assert.Contains("testType", body);
    }
}

/// <summary>
/// Boots the real API with the DbContext swapped to a shared SQLite in-memory database
/// and the seed users created up front. Environment "Testing" makes Program.cs skip the
/// SQL Server migrate/seed loop.
/// </summary>
public class LisApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _connection.Open();
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove the SQL Server DbContext registration.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<LisDbContext>) ||
                    d.ServiceType == typeof(LisDbContext))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<LisDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LisDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var tech = new User { Id = Guid.NewGuid(), Username = "tech", Role = UserRole.Technician };
            tech.PasswordHash = hasher.HashPassword(tech, "Tech123!");
            db.Users.Add(tech);
            await db.SaveChangesAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}

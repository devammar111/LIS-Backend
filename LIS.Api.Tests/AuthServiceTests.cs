using LIS.Api.Data;
using LIS.Api.Models;
using LIS.Api.Repositories;
using LIS.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LIS.Api.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly LisDbContext _db;
    private readonly AuthService _service;
    private readonly IPasswordHasher<User> _hasher = new PasswordHasher<User>();

    public AuthServiceTests()
    {
        _db = _factory.Create();
        SeedUser("tech", "Tech123!", UserRole.Technician);

        var jwt = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "LIS.Api",
            Audience = "LIS.Client",
            Key = "test-only-super-secret-signing-key-that-is-long-enough-32+",
            ExpiryMinutes = 60
        }));

        _service = new AuthService(
            new UserRepository(_db), _hasher, jwt, _db, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_AndWritesSuccessAudit_WhenCredentialsValid()
    {
        var response = await _service.LoginAsync(new LoginRequest("tech", "Tech123!"));

        Assert.NotNull(response);
        Assert.Equal("tech", response!.Username);
        Assert.Equal("Technician", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Contains(_db.AuditLogs, a => a.Action == "LoginSucceeded" && a.Username == "tech");
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_AndWritesFailedAudit_WhenPasswordWrong()
    {
        var response = await _service.LoginAsync(new LoginRequest("tech", "wrong"));

        Assert.Null(response);
        Assert.Contains(_db.AuditLogs, a => a.Action == "LoginFailed");
    }

    private void SeedUser(string username, string password, UserRole role)
    {
        var user = new User { Id = Guid.NewGuid(), Username = username, Role = role };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _db.Users.Add(user);
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _factory.Dispose();
    }
}

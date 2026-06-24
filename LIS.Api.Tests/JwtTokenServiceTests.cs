using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LIS.Api.Models;
using LIS.Api.Services;
using Microsoft.Extensions.Options;

namespace LIS.Api.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service = new(Options.Create(new JwtOptions
    {
        Issuer = "LIS.Api",
        Audience = "LIS.Client",
        Key = "test-only-super-secret-signing-key-that-is-long-enough-32+",
        ExpiryMinutes = 60
    }));

    [Fact]
    public void CreateToken_EmbedsUsernameAndRoleClaims()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "admin", Role = UserRole.Admin };

        var (token, expiresAtUtc) = _service.CreateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("admin", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("LIS.Api", jwt.Issuer);
        Assert.True(expiresAtUtc > DateTime.UtcNow.AddMinutes(58));
        Assert.True(expiresAtUtc < DateTime.UtcNow.AddMinutes(62));
    }
}

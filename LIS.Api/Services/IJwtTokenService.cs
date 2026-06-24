using LIS.Api.Models;

namespace LIS.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}

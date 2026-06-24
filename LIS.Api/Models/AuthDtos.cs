namespace LIS.Api.Models;

public record LoginResponse(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAtUtc);

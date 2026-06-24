using LIS.Api.Models;

namespace LIS.Api.Services;

public interface IAuthService
{
    /// <summary>Returns a login response on success, or null when credentials are invalid.</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

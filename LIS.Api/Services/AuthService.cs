using LIS.Api.Data;
using LIS.Api.Models;
using LIS.Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace LIS.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LisDbContext _db;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService,
        LisDbContext db,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _db = db;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
        {
            await WriteAuditAsync("LoginFailed", user, request.Username, cancellationToken);
            _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
            return null;
        }

        await WriteAuditAsync("LoginSucceeded", user, user.Username, cancellationToken);
        _logger.LogInformation("User {Username} logged in", user.Username);

        var (token, expiresAtUtc) = _jwtTokenService.CreateToken(user);
        return new LoginResponse(token, user.Username, user.Role.ToString(), expiresAtUtc);
    }

    private async Task WriteAuditAsync(string action, User? user, string username, CancellationToken cancellationToken)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user?.Id,
            Username = username,
            Action = action,
            EntityType = nameof(User),
            EntityId = user?.Id.ToString(),
            TimestampUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}

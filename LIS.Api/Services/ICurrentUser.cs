namespace LIS.Api.Services;

/// <summary>Abstraction over the authenticated principal for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}

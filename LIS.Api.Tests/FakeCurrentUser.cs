using LIS.Api.Services;

namespace LIS.Api.Tests;

public class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(Guid? userId = null, string? username = null)
    {
        UserId = userId;
        Username = username;
    }

    public Guid? UserId { get; }
    public string? Username { get; }
    public bool IsAuthenticated => UserId is not null;
}

using LIS.Api.Data;
using LIS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LIS.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LisDbContext _db;

    public UserRepository(LisDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
}

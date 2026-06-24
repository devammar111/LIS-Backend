using LIS.Api.Data;
using LIS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LIS.Api.Services;

public class AuditService : IAuditService
{
    private readonly LisDbContext _db;

    public AuditService(LisDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogResponse(
                a.Id, a.UserId, a.Username, a.Action, a.EntityType, a.EntityId, a.TimestampUtc, a.Details))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResponse<AuditLogResponse>(items, totalCount, page, pageSize, totalPages);
    }
}

using LIS.Api.Models;

namespace LIS.Api.Services;

public interface IAuditService
{
    Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

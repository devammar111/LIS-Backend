using LIS.Api.Models;

namespace LIS.Api.Repositories;

public interface IOrderRepository
{
    Task<LabOrder> AddAsync(LabOrder order, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<LabOrder> Items, int TotalCount)> GetPagedAsync(
        Priority? priorityFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

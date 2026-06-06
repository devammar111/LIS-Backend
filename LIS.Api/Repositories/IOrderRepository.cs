using LIS.Api.Models;

namespace LIS.Api.Repositories;

public interface IOrderRepository
{
    Task<LabOrder> AddAsync(LabOrder order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LabOrder>> GetAllAsync(CancellationToken cancellationToken = default);
}

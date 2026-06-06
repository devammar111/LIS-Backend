using System.Collections.Concurrent;
using LIS.Api.Models;

namespace LIS.Api.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentBag<LabOrder> _orders = new();

    public Task<LabOrder> AddAsync(LabOrder order, CancellationToken cancellationToken = default)
    {
        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<LabOrder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LabOrder> snapshot = _orders.ToList();
        return Task.FromResult(snapshot);
    }
}

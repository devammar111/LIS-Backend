using LIS.Api.Data;
using LIS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LIS.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly LisDbContext _db;

    public OrderRepository(LisDbContext db)
    {
        _db = db;
    }

    public async Task<LabOrder> AddAsync(LabOrder order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<(IReadOnlyList<LabOrder> Items, int TotalCount)> GetPagedAsync(
        Priority? priorityFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.AsNoTracking();

        if (priorityFilter is { } priority)
        {
            query = query.Where(o => o.Priority == priority);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CollectionDate)
            .ThenByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

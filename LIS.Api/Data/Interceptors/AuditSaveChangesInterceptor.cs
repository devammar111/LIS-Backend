using System.Text.Json;
using LIS.Api.Models;
using LIS.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LIS.Api.Data.Interceptors;

/// <summary>
/// Writes an <see cref="AuditLog"/> row for every domain-entity write that flows through
/// SaveChanges. Auth events (which have no domain entity) are audited explicitly in AuthService.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is not null)
        {
            AddAuditEntries(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AddAuditEntries(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void AddAuditEntries(DbContext context)
    {
        // Snapshot first so we don't enumerate the change tracker while mutating it.
        var newOrders = context.ChangeTracker.Entries<LabOrder>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        if (newOrders.Count == 0)
        {
            return;
        }

        foreach (var order in newOrders)
        {
            context.Set<AuditLog>().Add(new AuditLog
            {
                UserId = _currentUser.UserId,
                Username = _currentUser.Username,
                Action = "OrderCreated",
                EntityType = nameof(LabOrder),
                EntityId = order.Id.ToString(),
                TimestampUtc = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(new
                {
                    order.PatientName,
                    TestType = order.TestType.ToDisplay(),
                    Priority = order.Priority.ToDisplay(),
                    CollectionDate = order.CollectionDate.ToString("yyyy-MM-dd")
                })
            });
        }
    }
}

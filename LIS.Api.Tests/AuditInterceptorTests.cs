using LIS.Api.Data;
using LIS.Api.Data.Interceptors;
using LIS.Api.Models;

namespace LIS.Api.Tests;

public class AuditInterceptorTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task SavingOrder_WritesSingleOrderCreatedAuditRow()
    {
        var userId = Guid.NewGuid();
        var interceptor = new AuditSaveChangesInterceptor(new FakeCurrentUser(userId, "tech"));
        using var db = _factory.Create(interceptor);

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            PatientName = "Jane Doe",
            TestType = TestType.CBC,
            Priority = Priority.STAT,
            CollectionDate = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var audit = Assert.Single(db.AuditLogs);
        Assert.Equal("OrderCreated", audit.Action);
        Assert.Equal(order.Id.ToString(), audit.EntityId);
        Assert.Equal("tech", audit.Username);
        Assert.Equal(userId, audit.UserId);
    }

    public void Dispose() => _factory.Dispose();
}

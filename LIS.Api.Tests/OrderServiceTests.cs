using LIS.Api.Data;
using LIS.Api.Models;
using LIS.Api.Repositories;
using LIS.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LIS.Api.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly LisDbContext _db;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _db = _factory.Create();
        var repository = new OrderRepository(_db);
        _service = new OrderService(
            repository,
            new FakeCurrentUser(Guid.NewGuid(), "tech"),
            NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task CreateOrderAsync_PersistsOrder_AndNormalizesEnums()
    {
        var request = new CreateOrderRequest
        {
            PatientName = "  John Smith  ",
            TestType = "cbc",
            Priority = "stat",
            CollectionDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var response = await _service.CreateOrderAsync(request);

        Assert.Equal("John Smith", response.PatientName);
        Assert.Equal("CBC", response.TestType);
        Assert.Equal("STAT", response.Priority);

        var persisted = Assert.Single(_db.Orders);
        Assert.Equal(TestType.CBC, persisted.TestType);
        Assert.Equal(Priority.STAT, persisted.Priority);
    }

    [Fact]
    public async Task CreateOrderAsync_MapsLipidPanelDisplayName()
    {
        var request = new CreateOrderRequest
        {
            PatientName = "Jane Doe",
            TestType = "Lipid Panel",
            Priority = "Routine",
            CollectionDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var response = await _service.CreateOrderAsync(request);

        Assert.Equal("Lipid Panel", response.TestType);
        Assert.Equal(TestType.LipidPanel, Assert.Single(_db.Orders).TestType);
    }

    [Fact]
    public async Task GetOrdersAsync_FiltersStatOrders_WhenPriorityIsHigh()
    {
        await SeedOrderAsync("A", TestType.CBC, Priority.Routine, DateTime.Today.AddDays(2));
        await SeedOrderAsync("B", TestType.BMP, Priority.STAT, DateTime.Today);

        var response = await _service.GetOrdersAsync(new OrderQueryParameters { Priority = "high" });

        Assert.Equal(1, response.TotalCount);
        Assert.Equal("STAT", Assert.Single(response.Items).Priority);
    }

    [Fact]
    public async Task GetOrdersAsync_SortsByCollectionDateDescending()
    {
        await SeedOrderAsync("Earlier", TestType.UA, Priority.Routine, DateTime.Today);
        await SeedOrderAsync("Later", TestType.UA, Priority.Routine, DateTime.Today.AddDays(5));

        var response = await _service.GetOrdersAsync(new OrderQueryParameters());

        Assert.Equal("Later", response.Items[0].PatientName);
        Assert.Equal("Earlier", response.Items[1].PatientName);
    }

    private async Task SeedOrderAsync(string name, TestType testType, Priority priority, DateTime collectionDate)
    {
        _db.Orders.Add(new LabOrder
        {
            Id = Guid.NewGuid(),
            PatientName = name,
            TestType = testType,
            Priority = priority,
            CollectionDate = DateOnly.FromDateTime(collectionDate),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
        _factory.Dispose();
    }
}

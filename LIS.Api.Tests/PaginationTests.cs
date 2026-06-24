using LIS.Api.Data;
using LIS.Api.Models;
using LIS.Api.Repositories;
using LIS.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LIS.Api.Tests;

public class PaginationTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();
    private readonly LisDbContext _db;
    private readonly OrderService _service;

    public PaginationTests()
    {
        _db = _factory.Create();
        _service = new OrderService(
            new OrderRepository(_db), new FakeCurrentUser(), NullLogger<OrderService>.Instance);
    }

    [Theory]
    [InlineData(0, 20)]              // page < 1 clamps to 1
    [InlineData(-5, 20)]
    public void QueryParameters_ClampPage(int input, int _)
    {
        var query = new OrderQueryParameters { Page = input };
        Assert.True(query.Page >= 1);
    }

    [Theory]
    [InlineData(0, 20)]             // below 1 -> default 20
    [InlineData(500, OrderQueryParameters.MaxPageSize)] // above cap -> 100
    public void QueryParameters_ClampPageSize(int input, int expected)
    {
        var query = new OrderQueryParameters { PageSize = input };
        Assert.Equal(expected, query.PageSize);
    }

    [Fact]
    public async Task GetOrdersAsync_ComputesTotalPages_AndPagesResults()
    {
        for (var i = 0; i < 25; i++)
        {
            await SeedOrderAsync($"P{i}", DateTime.Today.AddDays(i));
        }

        var page1 = await _service.GetOrdersAsync(new OrderQueryParameters { Page = 1, PageSize = 10 });

        Assert.Equal(25, page1.TotalCount);
        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(3, page1.TotalPages);

        var page3 = await _service.GetOrdersAsync(new OrderQueryParameters { Page = 3, PageSize = 10 });
        Assert.Equal(5, page3.Items.Count);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsConsistentShape_WhenFilteredVsUnfiltered()
    {
        await SeedOrderAsync("Routine", DateTime.Today, Priority.Routine);
        await SeedOrderAsync("Stat", DateTime.Today, Priority.STAT);

        var unfiltered = await _service.GetOrdersAsync(new OrderQueryParameters());
        var filtered = await _service.GetOrdersAsync(new OrderQueryParameters { Priority = "high" });

        Assert.Equal(2, unfiltered.TotalCount);
        Assert.Equal(1, filtered.TotalCount);
        // Same envelope fields populated in both cases.
        Assert.Equal(1, filtered.Page);
        Assert.Equal(20, filtered.PageSize);
    }

    private async Task SeedOrderAsync(string name, DateTime collectionDate, Priority priority = Priority.Routine)
    {
        _db.Orders.Add(new LabOrder
        {
            Id = Guid.NewGuid(),
            PatientName = name,
            TestType = TestType.CBC,
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

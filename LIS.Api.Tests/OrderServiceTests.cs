using Moq;
using LIS.Api.Models;
using LIS.Api.Repositories;
using LIS.Api.Services;

namespace LIS.Api.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = new OrderService(_repository.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsValidationErrors_WhenFieldsAreInvalid()
    {
        var request = new CreateOrderRequest
        {
            PatientName = "",
            TestType = "Invalid",
            Priority = "Urgent",
            CollectionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };

        var (_, errors) = await _service.CreateOrderAsync(request);

        Assert.NotNull(errors);
        Assert.Contains("patientName", errors!.Keys);
        Assert.Contains("testType", errors.Keys);
        Assert.Contains("priority", errors.Keys);
        Assert.Contains("collectionDate", errors.Keys);
        _repository.Verify(repo => repo.AddAsync(It.IsAny<LabOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_PersistsOrder_WhenRequestIsValid()
    {
        var request = new CreateOrderRequest
        {
            PatientName = "John Smith",
            TestType = "cbc",
            Priority = "stat",
            CollectionDate = DateOnly.FromDateTime(DateTime.Today)
        };

        _repository
            .Setup(repo => repo.AddAsync(It.IsAny<LabOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LabOrder order, CancellationToken _) => order);

        var (order, errors) = await _service.CreateOrderAsync(request);

        Assert.Null(errors);
        Assert.NotNull(order);
        Assert.Equal("John Smith", order!.PatientName);
        Assert.Equal("CBC", order.TestType);
        Assert.Equal("STAT", order.Priority);
    }

    [Fact]
    public async Task GetOrdersAsync_FiltersStatOrders_WhenPriorityIsHigh()
    {
        var orders = new List<LabOrder>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientName = "A",
                TestType = "CBC",
                Priority = "Routine",
                CollectionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                PatientName = "B",
                TestType = "BMP",
                Priority = "STAT",
                CollectionDate = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = DateTime.UtcNow
            }
        };

        _repository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var response = await _service.GetOrdersAsync("high");

        Assert.Single(response.Orders);
        Assert.Equal("STAT", response.Orders[0].Priority);
    }

    [Fact]
    public async Task GetOrdersAsync_SortsByCollectionDateDescending()
    {
        var orders = new List<LabOrder>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientName = "Earlier",
                TestType = "UA",
                Priority = "Routine",
                CollectionDate = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                PatientName = "Later",
                TestType = "UA",
                Priority = "Routine",
                CollectionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                CreatedAt = DateTime.UtcNow
            }
        };

        _repository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var response = await _service.GetOrdersAsync(null);

        Assert.Equal("Later", response.Orders[0].PatientName);
        Assert.Equal("Earlier", response.Orders[1].PatientName);
    }
}

using Microsoft.AspNetCore.Mvc;
using Moq;
using LIS.Api.Controllers;
using LIS.Api.Models;
using LIS.Api.Services;

namespace LIS.Api.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderService = new();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(_orderService.Object);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenValidationFails()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["patientName"] = ["Patient name is required."]
        };

        _orderService
            .Setup(service => service.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, errors));

        var result = await _controller.CreateOrder(new CreateOrderRequest(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var body = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.Contains("patientName", body.Errors.Keys);
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreated_WhenOrderIsValid()
    {
        var order = new OrderResponse(
            Guid.NewGuid(),
            "Jane Doe",
            "CBC",
            "STAT",
            DateOnly.FromDateTime(DateTime.Today),
            DateTime.UtcNow);

        _orderService
            .Setup(service => service.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((order, null));

        var result = await _controller.CreateOrder(new CreateOrderRequest
        {
            PatientName = "Jane Doe",
            TestType = "CBC",
            Priority = "STAT",
            CollectionDate = DateOnly.FromDateTime(DateTime.Today)
        }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task GetOrders_ReturnsOk_WithOrderListResponse()
    {
        var response = new OrderListResponse([
            new OrderResponse(Guid.NewGuid(), "Jane Doe", "BMP", "Routine", DateOnly.FromDateTime(DateTime.Today), DateTime.UtcNow)
        ]);

        _orderService
            .Setup(service => service.GetOrdersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetOrders(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<OrderListResponse>(okResult.Value);
        Assert.Single(body.Orders);
    }
}

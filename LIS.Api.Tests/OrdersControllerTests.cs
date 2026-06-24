using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using LIS.Api.Controllers;
using LIS.Api.Models;
using LIS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LIS.Api.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderService = new();
    private readonly Mock<IValidator<CreateOrderRequest>> _validator = new();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(_orderService.Object, _validator.Object)
        {
            ProblemDetailsFactory = new TestProblemDetailsFactory(),
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task CreateOrder_ReturnsValidationProblem_WhenValidationFails()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("PatientName", "Patient name is required.")
            }));

        var result = await _controller.CreateOrder(new CreateOrderRequest(), CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Contains("patientName", problem.Errors.Keys);
        _orderService.Verify(
            s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreated_WhenOrderIsValid()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var order = new OrderResponse(
            Guid.NewGuid(), "Jane Doe", "CBC", "STAT", DateOnly.FromDateTime(DateTime.Today), DateTime.UtcNow);
        _orderService
            .Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

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
    public async Task GetOrders_ReturnsOk_WithPagedResponse()
    {
        var paged = new PagedResponse<OrderResponse>(
            new[]
            {
                new OrderResponse(Guid.NewGuid(), "Jane Doe", "BMP", "Routine",
                    DateOnly.FromDateTime(DateTime.Today), DateTime.UtcNow)
            },
            TotalCount: 1, Page: 1, PageSize: 20, TotalPages: 1);

        _orderService
            .Setup(s => s.GetOrdersAsync(It.IsAny<OrderQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _controller.GetOrders(new OrderQueryParameters(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<PagedResponse<OrderResponse>>(ok.Value);
        Assert.Single(body.Items);
    }

    [Fact]
    public void OrdersController_RequiresAuthorization()
    {
        var attribute = typeof(OrdersController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
    }
}

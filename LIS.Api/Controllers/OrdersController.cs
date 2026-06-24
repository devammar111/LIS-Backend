using FluentValidation;
using LIS.Api.Models;
using LIS.Api.Services;
using LIS.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _validator;

    public OrdersController(IOrderService orderService, IValidator<CreateOrderRequest> validator)
    {
        _orderService = orderService;
        _validator = validator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var order = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderQueryParameters query,
        CancellationToken cancellationToken)
    {
        var response = await _orderService.GetOrdersAsync(query, cancellationToken);
        return Ok(response);
    }
}

using Microsoft.AspNetCore.Mvc;
using LIS.Api.Models;
using LIS.Api.Services;

namespace LIS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var (order, errors) = await _orderService.CreateOrderAsync(request, cancellationToken);

        if (errors is not null)
        {
            return BadRequest(new ValidationErrorResponse(errors));
        }

        return CreatedAtAction(nameof(GetOrders), new { id = order!.Id }, order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? priority,
        CancellationToken cancellationToken)
    {
        var response = await _orderService.GetOrdersAsync(priority, cancellationToken);
        return Ok(response);
    }
}

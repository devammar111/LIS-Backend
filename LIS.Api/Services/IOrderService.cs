using LIS.Api.Models;

namespace LIS.Api.Services;

public interface IOrderService
{
    Task<(OrderResponse? Order, IDictionary<string, string[]>? Errors)> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<OrderListResponse> GetOrdersAsync(
        string? priorityFilter,
        CancellationToken cancellationToken = default);
}

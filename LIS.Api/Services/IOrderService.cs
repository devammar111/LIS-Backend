using LIS.Api.Models;

namespace LIS.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<OrderResponse>> GetOrdersAsync(
        OrderQueryParameters query,
        CancellationToken cancellationToken = default);
}

using LIS.Api.Models;
using LIS.Api.Repositories;

namespace LIS.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ICurrentUser currentUser,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Input has already been validated (FluentValidation) before reaching here,
        // so the enum parses are guaranteed to succeed.
        EnumDisplay.TryParseTestType(request.TestType, out var testType);
        EnumDisplay.TryParsePriority(request.Priority, out var priority);

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            PatientName = request.PatientName.Trim(),
            TestType = testType,
            Priority = priority,
            CollectionDate = request.CollectionDate,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        await _repository.AddAsync(order, cancellationToken);
        _logger.LogInformation(
            "Order {OrderId} created by {Username}", order.Id, _currentUser.Username ?? "unknown");

        return ToResponse(order);
    }

    public async Task<PagedResponse<OrderResponse>> GetOrdersAsync(
        OrderQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var priorityFilter = ResolvePriorityFilter(query.Priority);

        var (items, totalCount) = await _repository.GetPagedAsync(
            priorityFilter, query.Page, query.PageSize, cancellationToken);

        var responses = items.Select(ToResponse).ToList();
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponse<OrderResponse>(
            responses, totalCount, query.Page, query.PageSize, totalPages);
    }

    /// <summary>
    /// Maps the optional priority query value to an enum filter.
    /// Accepts the legacy "high" alias (= STAT) plus "STAT"/"Routine"; anything else = no filter.
    /// </summary>
    private static Priority? ResolvePriorityFilter(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            return null;
        }

        if (priority.Equals("high", StringComparison.OrdinalIgnoreCase))
        {
            return Priority.STAT;
        }

        return EnumDisplay.TryParsePriority(priority, out var parsed) ? parsed : null;
    }

    private static OrderResponse ToResponse(LabOrder order) =>
        new(order.Id,
            order.PatientName,
            order.TestType.ToDisplay(),
            order.Priority.ToDisplay(),
            order.CollectionDate,
            order.CreatedAt);
}

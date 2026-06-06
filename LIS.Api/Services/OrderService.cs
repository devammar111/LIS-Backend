using LIS.Api.Models;
using LIS.Api.Repositories;

namespace LIS.Api.Services;

public class OrderService : IOrderService
{
    private static readonly HashSet<string> AllowedTestTypes =
        new(StringComparer.OrdinalIgnoreCase) { "CBC", "BMP", "Lipid Panel", "UA" };

    private static readonly HashSet<string> AllowedPriorities =
        new(StringComparer.OrdinalIgnoreCase) { "Routine", "STAT" };

    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<(OrderResponse? Order, IDictionary<string, string[]>? Errors)> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateRequest(request);
        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            PatientName = request.PatientName.Trim(),
            TestType = NormalizeTestType(request.TestType),
            Priority = NormalizePriority(request.Priority),
            CollectionDate = request.CollectionDate,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(order, cancellationToken);
        return (ToResponse(order), null);
    }

    public async Task<OrderListResponse> GetOrdersAsync(
        string? priorityFilter,
        CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetAllAsync(cancellationToken);

        IEnumerable<LabOrder> query = orders;

        if (IsHighPriorityFilter(priorityFilter))
        {
            query = query.Where(order =>
                order.Priority.Equals("STAT", StringComparison.OrdinalIgnoreCase));
        }

        var results = query
            .OrderByDescending(order => order.CollectionDate)
            .ThenByDescending(order => order.CreatedAt)
            .Select(ToResponse)
            .ToList();

        return new OrderListResponse(results);
    }

    private static Dictionary<string, string[]> ValidateRequest(CreateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.PatientName))
        {
            errors["patientName"] = ["Patient name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.TestType))
        {
            errors["testType"] = ["Test type is required."];
        }
        else if (!AllowedTestTypes.Contains(request.TestType.Trim()))
        {
            errors["testType"] = ["Test type must be one of: CBC, BMP, Lipid Panel, UA."];
        }

        if (string.IsNullOrWhiteSpace(request.Priority))
        {
            errors["priority"] = ["Priority is required."];
        }
        else if (!AllowedPriorities.Contains(request.Priority.Trim()))
        {
            errors["priority"] = ["Priority must be Routine or STAT."];
        }

        if (request.CollectionDate == default)
        {
            errors["collectionDate"] = ["Collection date is required."];
        }
        else if (request.CollectionDate < DateOnly.FromDateTime(DateTime.Today))
        {
            errors["collectionDate"] = ["Collection date cannot be in the past."];
        }

        return errors;
    }

    private static bool IsHighPriorityFilter(string? priorityFilter) =>
        !string.IsNullOrWhiteSpace(priorityFilter) &&
        priorityFilter.Equals("high", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeTestType(string testType) =>
        AllowedTestTypes.First(value => value.Equals(testType.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizePriority(string priority) =>
        AllowedPriorities.First(value => value.Equals(priority.Trim(), StringComparison.OrdinalIgnoreCase));

    private static OrderResponse ToResponse(LabOrder order) =>
        new(order.Id, order.PatientName, order.TestType, order.Priority, order.CollectionDate, order.CreatedAt);
}

namespace LIS.Api.Models;

public record OrderResponse(
    Guid Id,
    string PatientName,
    string TestType,
    string Priority,
    DateOnly CollectionDate,
    DateTime CreatedAt);

public record OrderListResponse(IReadOnlyList<OrderResponse> Orders);

public record ValidationErrorResponse(IDictionary<string, string[]> Errors);

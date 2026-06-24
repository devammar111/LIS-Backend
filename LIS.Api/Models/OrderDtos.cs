namespace LIS.Api.Models;

public record OrderResponse(
    Guid Id,
    string PatientName,
    string TestType,
    string Priority,
    DateOnly CollectionDate,
    DateTime CreatedAt);

/// <summary>
/// Consistent paged envelope returned by list endpoints regardless of any active filter.
/// </summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

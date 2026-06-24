namespace LIS.Api.Models;

/// <summary>Query parameters for listing orders. Bound from the query string.</summary>
public class OrderQueryParameters
{
    public const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>
    /// Optional priority filter. Accepts the legacy "high" alias (= STAT), or "STAT"/"Routine".
    /// </summary>
    public string? Priority { get; set; }
}

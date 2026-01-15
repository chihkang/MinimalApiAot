namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Pagination request parameters
/// </summary>
public record PaginationRequest
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Page number (1-based, default: 1)
    /// </summary>
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? DefaultPageSize : (value > MaxPageSize ? MaxPageSize : value);
    }

    /// <summary>
    /// Calculate skip count for database query
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}

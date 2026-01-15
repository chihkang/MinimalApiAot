namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// Items for the current page
    /// </summary>
    [JsonPropertyName("items")]
    public required IEnumerable<T> Items { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    [JsonPropertyName("totalCount")]
    public required int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    [JsonPropertyName("page")]
    public required int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [JsonPropertyName("pageSize")]
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there are more pages after the current one
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there are pages before the current one
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Create a paginated response from items
    /// </summary>
    public static PaginatedResponse<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

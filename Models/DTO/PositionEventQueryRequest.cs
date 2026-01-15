namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Query parameters for filtering position events by user
/// </summary>
public record PositionEventQueryRequest
{
    /// <summary>
    /// Filter by event type (BUY, SELL, REPLACE). If null, returns all types.
    /// </summary>
    public PositionEventType? Type { get; init; }

    /// <summary>
    /// Start date for filtering (inclusive). Defaults to 1 year ago if not provided.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for filtering (inclusive). Defaults to current date if not provided.
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Page number (1-based, default: 1)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Get effective start date (defaults to 1 year ago)
    /// </summary>
    public DateTime GetEffectiveStartDate() => StartDate ?? DateTime.UtcNow.AddYears(-1);

    /// <summary>
    /// Get effective end date (defaults to now)
    /// </summary>
    public DateTime GetEffectiveEndDate() => EndDate ?? DateTime.UtcNow;

    /// <summary>
    /// Convert to PaginationRequest
    /// </summary>
    public PaginationRequest ToPaginationRequest() => new() { Page = Page, PageSize = PageSize };
}

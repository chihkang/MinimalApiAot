namespace MinimalApiAot.Interfaces;

/// <summary>
/// Service interface for position event operations
/// </summary>
public interface IPositionEventService
{
    /// <summary>
    /// Get all position events with pagination
    /// </summary>
    Task<PaginatedResponse<PositionEventResponseDto>> GetAllAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a position event by ID
    /// </summary>
    Task<PositionEvent?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a position event by operation ID
    /// </summary>
    Task<PositionEvent?> GetByOperationIdAsync(string operationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position events by user ID with filtering and pagination (default: last 1 year)
    /// </summary>
    Task<PaginatedResponse<PositionEventResponseDto>> GetByUserIdAsync(
        ObjectId userId,
        PositionEventQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position events by stock ID with filtering and pagination
    /// </summary>
    Task<PaginatedResponse<PositionEventResponseDto>> GetByStockIdAsync(
        ObjectId stockId,
        PositionEventQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new position event and sync Portfolio quantity
    /// </summary>
    /// <returns>Created position event or error result</returns>
    Task<PositionEventCreateResult> CreateAsync(CreatePositionEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing position event (for corrections) and sync Portfolio quantity
    /// </summary>
    Task<PositionEventUpdateResult> UpdateAsync(ObjectId id, UpdatePositionEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a position event and rollback Portfolio quantity
    /// </summary>
    Task<PositionEventDeleteResult> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of position event creation
/// </summary>
public record PositionEventCreateResult
{
    public bool Success { get; init; }
    public PositionEvent? PositionEvent { get; init; }
    public string? ErrorMessage { get; init; }
    public PositionEventErrorType? ErrorType { get; init; }

    public static PositionEventCreateResult Ok(PositionEvent positionEvent) => new()
    {
        Success = true,
        PositionEvent = positionEvent
    };

    public static PositionEventCreateResult Error(string message, PositionEventErrorType type) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = type
    };
}

/// <summary>
/// Result of position event update
/// </summary>
public record PositionEventUpdateResult
{
    public bool Success { get; init; }
    public PositionEvent? PositionEvent { get; init; }
    public string? ErrorMessage { get; init; }
    public PositionEventErrorType? ErrorType { get; init; }

    public static PositionEventUpdateResult Ok(PositionEvent positionEvent) => new()
    {
        Success = true,
        PositionEvent = positionEvent
    };

    public static PositionEventUpdateResult Error(string message, PositionEventErrorType type) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = type
    };
}

/// <summary>
/// Result of position event deletion
/// </summary>
public record PositionEventDeleteResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public PositionEventErrorType? ErrorType { get; init; }

    public static PositionEventDeleteResult Ok() => new() { Success = true };

    public static PositionEventDeleteResult Error(string message, PositionEventErrorType type) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorType = type
    };
}

/// <summary>
/// Types of errors that can occur during position event operations
/// </summary>
public enum PositionEventErrorType
{
    /// <summary>Validation failed (e.g., invalid quantity calculation)</summary>
    ValidationFailed,
    
    /// <summary>Duplicate operationId</summary>
    DuplicateOperationId,
    
    /// <summary>Position event not found</summary>
    NotFound,
    
    /// <summary>User not found</summary>
    UserNotFound,
    
    /// <summary>Stock not found</summary>
    StockNotFound,
    
    /// <summary>Portfolio not found</summary>
    PortfolioNotFound,
    
    /// <summary>Concurrency conflict after retries</summary>
    ConcurrencyConflict,
    
    /// <summary>Database error</summary>
    DatabaseError
}

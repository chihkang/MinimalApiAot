namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Simple error response with message
/// </summary>
public record ErrorResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("errorType")]
    public string? ErrorType { get; init; }

    public static ErrorResponse Create(string message) => new() { Message = message };
    
    public static ErrorResponse Create(string message, string errorType) => new() 
    { 
        Message = message, 
        ErrorType = errorType 
    };
}

/// <summary>
/// Simple success response with message
/// </summary>
public record SuccessResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    public static SuccessResponse Create(string message) => new() { Message = message };
}

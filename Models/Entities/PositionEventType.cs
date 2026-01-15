namespace MinimalApiAot.Models.Entities;

/// <summary>
/// Type of position event operation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PositionEventType>))]
public enum PositionEventType
{
    /// <summary>
    /// Buy operation - increases position quantity
    /// </summary>
    BUY,
    
    /// <summary>
    /// Sell operation - decreases position quantity
    /// </summary>
    SELL,
    
    /// <summary>
    /// Replace operation - completely replaces position quantity
    /// </summary>
    REPLACE
}

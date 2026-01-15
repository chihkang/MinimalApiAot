namespace MinimalApiAot.Models.Entities;

/// <summary>
/// Supported currencies for position events
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Currency>))]
public enum Currency
{
    /// <summary>
    /// Taiwan Dollar
    /// </summary>
    TWD,
    
    /// <summary>
    /// US Dollar
    /// </summary>
    USD
}

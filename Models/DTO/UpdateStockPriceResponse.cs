namespace MinimalApiAot.Models.DTO;

public record UpdateStockPriceResponse
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("oldPrice")]
    public decimal OldPrice { get; init; }

    [JsonPropertyName("newPrice")]
    public decimal NewPrice { get; init; }

    [JsonPropertyName("currency")]
    public required string Currency { get; init; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; init; }
}
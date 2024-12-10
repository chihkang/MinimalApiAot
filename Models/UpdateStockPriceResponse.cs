namespace MinimalApiAot.Models;

public record UpdateStockPriceResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("oldPrice")]
    public decimal OldPrice { get; init; }

    [JsonPropertyName("newPrice")]
    public decimal NewPrice { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; init; }
}
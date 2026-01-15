namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Response DTO for position event data
/// </summary>
public record PositionEventResponseDto
{
    [JsonPropertyName("_id")]
    public required string Id { get; init; }

    [JsonPropertyName("operationId")]
    public required string OperationId { get; init; }

    [JsonPropertyName("userId")]
    public required string UserId { get; init; }

    [JsonPropertyName("stockId")]
    public required string StockId { get; init; }

    [JsonPropertyName("type")]
    public required PositionEventType Type { get; init; }

    [JsonPropertyName("tradeAt")]
    public required DateTime TradeAt { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("quantityBefore")]
    public required decimal QuantityBefore { get; init; }

    [JsonPropertyName("quantityAfter")]
    public required decimal QuantityAfter { get; init; }

    [JsonPropertyName("quantityDelta")]
    public required decimal QuantityDelta { get; init; }

    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    [JsonPropertyName("totalCostBefore")]
    public required decimal TotalCostBefore { get; init; }

    [JsonPropertyName("totalCostAfter")]
    public required decimal TotalCostAfter { get; init; }

    [JsonPropertyName("unitPrice")]
    public required decimal UnitPrice { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("appVersion")]
    public required string AppVersion { get; init; }

    /// <summary>
    /// Maps a PositionEvent entity to PositionEventResponseDto
    /// </summary>
    public static PositionEventResponseDto FromEntity(PositionEvent entity)
    {
        return new PositionEventResponseDto
        {
            Id = entity.Id.ToString(),
            OperationId = entity.OperationId,
            UserId = entity.UserId.ToString(),
            StockId = entity.StockId.ToString(),
            Type = entity.Type,
            TradeAt = entity.TradeAt,
            CreatedAt = entity.CreatedAt,
            QuantityBefore = entity.QuantityBefore,
            QuantityAfter = entity.QuantityAfter,
            QuantityDelta = entity.QuantityDelta,
            Currency = entity.Currency,
            TotalCostBefore = entity.TotalCostBefore,
            TotalCostAfter = entity.TotalCostAfter,
            UnitPrice = entity.UnitPrice,
            Source = entity.Source,
            AppVersion = entity.AppVersion
        };
    }
}

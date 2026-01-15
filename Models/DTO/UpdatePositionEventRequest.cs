namespace MinimalApiAot.Models.DTO;

/// <summary>
/// Request to update an existing position event (for corrections)
/// </summary>
public record UpdatePositionEventRequest
{
    /// <summary>
    /// Quantity held before this operation
    /// </summary>
    public decimal? QuantityBefore { get; init; }

    /// <summary>
    /// Quantity held after this operation
    /// </summary>
    public decimal? QuantityAfter { get; init; }

    /// <summary>
    /// Change in quantity
    /// </summary>
    public decimal? QuantityDelta { get; init; }

    /// <summary>
    /// Total cost basis before this operation
    /// </summary>
    public decimal? TotalCostBefore { get; init; }

    /// <summary>
    /// Total cost basis after this operation
    /// </summary>
    public decimal? TotalCostAfter { get; init; }

    /// <summary>
    /// Price per unit for this transaction
    /// </summary>
    public decimal? UnitPrice { get; init; }
}

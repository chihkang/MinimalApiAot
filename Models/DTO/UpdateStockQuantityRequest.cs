namespace MinimalApiAot.Models.DTO;

public record UpdateStockQuantityRequest
{
    public required decimal Quantity { get; init; }
}
namespace MinimalApiAot.Models.DTO;

public record UpdateStockByIdRequest
{
    public required string StockId { get; init; }
    public required decimal Quantity { get; init; }
}

public record UpdateStockByNameRequest
{
    public required string StockNameOrAlias { get; init; }
    public required decimal Quantity { get; init; }
}
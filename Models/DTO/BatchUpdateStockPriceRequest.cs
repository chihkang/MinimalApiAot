namespace MinimalApiAot.Models.DTO;

public record UpdateStockPriceItem
{
    public required string StockId { get; init; }
    public required decimal NewPrice { get; init; }
}

public record BatchUpdateStockPriceRequest
{
    public required List<UpdateStockPriceItem> Updates { get; init; }
}

public record BatchUpdateStockPriceResponse
{
    public List<UpdateStockPriceResponse> UpdatedStocks { get; init; } = [];
    public List<string> NotFoundIds { get; init; } = [];
    public List<string> InvalidIds { get; init; } = [];
}

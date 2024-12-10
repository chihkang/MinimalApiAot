namespace MinimalApiAot.Models.DTO;

public record PortfolioResponseDto
{
    public required ObjectId Id { get; init; }
    public required ObjectId UserId { get; init; }
    public required List<PortfolioStockDto> Stocks { get; init; }
    public required DateTime LastUpdated { get; init; }
}

public record PortfolioStockDto
{
    public required ObjectId StockId { get; init; }
    public required decimal Quantity { get; init; }
}

public record UpdatePortfolioStockDto
{
    public required ObjectId StockId { get; init; }
    public required decimal Quantity { get; init; }
}
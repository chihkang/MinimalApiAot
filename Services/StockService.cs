namespace MinimalApiAot.Services;

public class StockService(ApplicationDbContext context) : IStockService
{
    public async Task<IEnumerable<StockMinimalDto>> GetAllStocksMinimalAsync()
    {
        return await context.Stocks
            .AsNoTracking()
            .Select(s => new StockMinimalDto(
                s.Id,
                s.Name,
                s.Alias))
            .ToListAsync();
    }

    public async Task<UpdateStockPriceResponse?> UpdateStockPriceAsync(string name, decimal newPrice)
    {
        var stock = await context.Stocks
            .FirstOrDefaultAsync(s => s.Name == name);

        if (stock == null) return null;

        var oldPrice = stock.Price;
        var now = DateTime.UtcNow;

        stock.Price = newPrice;
        stock.LastUpdated = now;

        await context.SaveChangesAsync();

        return new UpdateStockPriceResponse
        {
            Name = name,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            Currency = stock.Currency,
            LastUpdated = now
        };
    }
}
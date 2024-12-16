namespace MinimalApiAot.Services;

public class StockService(ApplicationDbContext context, ILogger<StockService> logger) : IStockService
{
    public async Task<Stock?> GetByIdAsync(ObjectId objectId)
    {
        try
        {
            return await context.Stocks.FindAsync(objectId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查詢股票時發生錯誤。Stock ID: {StockId}", objectId);
            throw;
        }
    }

    public async Task<Stock?> GetByNameOrAliasAsync(string nameOrAlias)
    {
        try
        {
            return await context.Stocks
                .FirstOrDefaultAsync(s => 
                    s.Name.ToLower() == nameOrAlias.ToLower() || 
                    s.Alias.ToLower() == nameOrAlias.ToLower());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "根據名稱或代號查詢股票時發生錯誤。Name/Alias: {NameOrAlias}", nameOrAlias);
            throw;
        }
    }

    public async Task<UpdateStockPriceResponse?> UpdateStockPriceAsync(ObjectId stockId, decimal newPrice)
    {
        var stock = await context.Stocks
            .FirstOrDefaultAsync(s => s.Id == stockId);
        if (stock == null) return null;

        var oldPrice = stock.Price;
        var now = DateTime.UtcNow;
        
        stock.Price = newPrice;
        stock.LastUpdated = now;
        
        await context.SaveChangesAsync();

        return new UpdateStockPriceResponse
        {
            Name = stock.Name,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            Currency = stock.Currency,
            LastUpdated = now
        };
    }

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
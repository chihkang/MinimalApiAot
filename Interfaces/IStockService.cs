namespace MinimalApiAot.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockMinimalDto>> GetAllStocksMinimalAsync();
    Task<UpdateStockPriceResponse?> UpdateStockPriceAsync(string name, decimal newPrice);
    Task<Stock?> GetByIdAsync(ObjectId id);
    Task<Stock?> GetByNameOrAliasAsync(string nameOrAlias);
}
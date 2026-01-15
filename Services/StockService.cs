using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MinimalApiAot.Services;

public class StockService(MongoDbContext db, ILogger<StockService> logger) : IStockService
{
    private readonly IMongoCollection<Stock> _stocks = db.Stocks;
    public async Task<Stock?> GetByIdAsync(ObjectId objectId)
    {
        try
        {
            return await _stocks.Find(s => s.Id == objectId).FirstOrDefaultAsync();
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
            var escaped = Regex.Escape(nameOrAlias);
            var regex = new BsonRegularExpression($"^{escaped}$", "i");
            var filter = Builders<Stock>.Filter.Or(
                Builders<Stock>.Filter.Regex(s => s.Name, regex),
                Builders<Stock>.Filter.Regex(s => s.Alias, regex));

            return await _stocks.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "根據名稱或代號查詢股票時發生錯誤。Name/Alias: {NameOrAlias}", nameOrAlias);
            throw;
        }
    }

    public async Task<UpdateStockPriceResponse?> UpdateStockPriceAsync(ObjectId stockId, decimal newPrice)
    {
        var now = DateTime.UtcNow;

        var update = Builders<Stock>.Update
            .Set(s => s.Price, newPrice)
            .Set(s => s.LastUpdated, now);

        var options = new FindOneAndUpdateOptions<Stock>
        {
            ReturnDocument = ReturnDocument.Before
        };

        var original = await _stocks.FindOneAndUpdateAsync(s => s.Id == stockId, update, options);
        if (original == null) return null;

        return new UpdateStockPriceResponse
        {
            Name = original.Name,
            OldPrice = original.Price,
            NewPrice = newPrice,
            Currency = original.Currency,
            LastUpdated = now
        };
    }

    public async Task<IEnumerable<StockMinimalDto>> GetAllStocksMinimalAsync()
    {
        var stocks = await _stocks.Find(FilterDefinition<Stock>.Empty)
            .ToListAsync();

        return stocks.Select(s => new StockMinimalDto(
            s.Id,
            s.Name,
            s.Alias));
    }

    public async Task<UpdateStockPriceResponse?> UpdateStockPriceAsync(string name, decimal newPrice)
    {
        var now = DateTime.UtcNow;

        var update = Builders<Stock>.Update
            .Set(s => s.Price, newPrice)
            .Set(s => s.LastUpdated, now);

        var options = new FindOneAndUpdateOptions<Stock>
        {
            ReturnDocument = ReturnDocument.Before
        };

        var original = await _stocks.FindOneAndUpdateAsync(s => s.Name == name, update, options);
        if (original == null) return null;

        return new UpdateStockPriceResponse
        {
            Name = original.Name,
            OldPrice = original.Price,
            NewPrice = newPrice,
            Currency = original.Currency,
            LastUpdated = now
        };
    }

    public async Task<BatchUpdateStockPriceResponse> UpdateStockPricesBatchAsync(List<UpdateStockPriceItem> updates)
    {
        var result = new BatchUpdateStockPriceResponse();
        var validUpdates = new Dictionary<ObjectId, decimal>();

        foreach (var update in updates)
        {
            if (ObjectId.TryParse(update.StockId, out var objectId))
            {
                validUpdates[objectId] = update.NewPrice;
            }
            else
            {
                result.InvalidIds.Add(update.StockId);
            }
        }

        if (validUpdates.Count == 0) return result;

        var stockIds = validUpdates.Keys.ToList();
        var stocks = await _stocks.Find(s => stockIds.Contains(s.Id)).ToListAsync();

        var foundIds = stocks.Select(s => s.Id).ToHashSet();
        foreach (var id in stockIds.Where(id => !foundIds.Contains(id)))
        {
            result.NotFoundIds.Add(id.ToString());
        }

        var now = DateTime.UtcNow;
        var updatesToWrite = new List<WriteModel<Stock>>();

        foreach (var stock in stocks)
        {
            var newPrice = validUpdates[stock.Id];
            var oldPrice = stock.Price;

            var filter = Builders<Stock>.Filter.Eq(s => s.Id, stock.Id);
            var update = Builders<Stock>.Update
                .Set(s => s.Price, newPrice)
                .Set(s => s.LastUpdated, now);

            updatesToWrite.Add(new UpdateOneModel<Stock>(filter, update));

            result.UpdatedStocks.Add(new UpdateStockPriceResponse
            {
                Name = stock.Name,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                Currency = stock.Currency,
                LastUpdated = now
            });
        }

        if (updatesToWrite.Count > 0)
        {
            await _stocks.BulkWriteAsync(updatesToWrite);
        }

        return result;
    }
}
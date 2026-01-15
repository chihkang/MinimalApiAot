using MongoDB.Driver;

namespace MinimalApiAot.Services;

public class PortfolioService(MongoDbContext db, ILogger<PortfolioService> logger)
    : IPortfolioService
{
    private readonly IMongoCollection<Portfolio> _portfolios = db.Portfolios;
    private readonly IMongoCollection<User> _users = db.Users;
    public void UpdatePortfolioStock(Portfolio portfolio, ObjectId stockId, decimal quantity)
    {
        var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);
        if (stockEntry is null)
        {
            portfolio.Stocks.Add(new PortfolioStock
            {
                StockId = stockId,
                Quantity = quantity
            });
        }
        else
        {
            stockEntry.Quantity = quantity;
        }
    }

    public async Task<IEnumerable<Portfolio>> GetAllAsync()
    {
        return await _portfolios.Find(FilterDefinition<Portfolio>.Empty).ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(ObjectId id)
    {
        return await _portfolios.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Portfolio?> GetByUserIdAsync(ObjectId userId)
    {
        return await _portfolios.Find(p => p.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<Portfolio?> GetByUserNameAsync(string userName)
    {
        // 首先找到符合的 User
        var user = await _users.Find(u => u.Username == userName).FirstOrDefaultAsync();

        // 然後查詢對應的 Portfolio
        return user == null
            ? null
            : await _portfolios.Find(p => p.UserId == user.Id).FirstOrDefaultAsync();
    }

    public async Task<Portfolio> CreateAsync(ObjectId userId)
    {
        var portfolio = new Portfolio
        {
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            LastUpdated = DateTime.UtcNow,
            Stocks = [],
            Version = 1
        };

        await _portfolios.InsertOneAsync(portfolio);
        return portfolio;
    }

    public async Task<bool> UpdateAsync(Portfolio portfolio)
    {
        try
        {
            var existingPortfolio = await _portfolios.Find(p => p.Id == portfolio.Id).FirstOrDefaultAsync();

            if (existingPortfolio == null)
                return false;

            // 更新基本屬性
            existingPortfolio.UserId = portfolio.UserId;

            // 更新股票集合：只更新變更的部分
            foreach (var updatedStock in portfolio.Stocks)
            {
                var existingStock = existingPortfolio.Stocks
                    .FirstOrDefault(s => s.StockId == updatedStock.StockId);

                if (existingStock == null)
                {
                    // 新增新的股票
                    existingPortfolio.Stocks.Add(updatedStock);
                }
                else
                {
                    // 更新既有股票
                    existingStock.Quantity = updatedStock.Quantity;
                }
            }

            existingPortfolio.LastUpdated = DateTime.UtcNow;
            var updateResult = await _portfolios.ReplaceOneAsync(
                p => p.Id == existingPortfolio.Id,
                existingPortfolio);

            return updateResult.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新投資組合時發生錯誤。Portfolio ID: {PortfolioId}", portfolio.Id);
            throw;
        }
    }
}
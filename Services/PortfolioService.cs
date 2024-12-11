using Microsoft.EntityFrameworkCore.Storage;

namespace MinimalApiAot.Services;

public class PortfolioService(ApplicationDbContext context, ILogger<PortfolioService> logger)
    : IPortfolioService
{
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
        return await context.Portfolios
            .Include(p => p.Stocks)
            .ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(ObjectId id)
    {
        return await context.Portfolios
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Portfolio?> GetByUserIdAsync(ObjectId userId)
    {
        return await context.Portfolios
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<Portfolio?> GetByUserNameAsync(string userName)
    {
        // 首先找到符合的 User
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Username == userName);

        // 然後查詢對應的 Portfolio
        return await context.Portfolios
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => user != null && p.UserId == user.Id);
    }

    public async Task<Portfolio> CreateAsync(ObjectId userId)
    {
        var portfolio = new Portfolio
        {
            UserId = userId,
            LastUpdated = DateTime.UtcNow,
            Stocks = []
        };

        await context.Portfolios.AddAsync(portfolio);
        await context.SaveChangesAsync();
        return portfolio;
    }

    public async Task<bool> UpdateAsync(Portfolio portfolio)
    {
        try
        {
            var existingPortfolio = await context.Portfolios
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.Id == portfolio.Id);

            if (existingPortfolio == null)
                return false;

            // 更新基本屬性
            context.Entry(existingPortfolio).CurrentValues.SetValues(portfolio);

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
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新投資組合時發生錯誤。Portfolio ID: {PortfolioId}", portfolio.Id);
            throw;
        }
    }
}
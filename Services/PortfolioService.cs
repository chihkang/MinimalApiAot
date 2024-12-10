namespace MinimalApiAot.Services;

public class PortfolioService(ApplicationDbContext context, ILogger<PortfolioService> logger)
    : IPortfolioService
{
    private readonly ILogger<PortfolioService> _logger = logger;

    public async Task<IEnumerable<Portfolio>> GetAllAsync()
    {
        return await context.Portfolios.ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(ObjectId id)
    {
        return await context.Portfolios.FindAsync(id);
    }

    public async Task<Portfolio?> GetByUserIdAsync(ObjectId userId)
    {
        return await context.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId);
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
                .FirstOrDefaultAsync(p => p.Id == portfolio.Id);

            if (existingPortfolio == null)
                return false;

            // 更新投資組合內容
            existingPortfolio.Stocks = portfolio.Stocks;
            existingPortfolio.LastUpdated = DateTime.UtcNow;

            // 執行更新操作
            context.Portfolios.Update(existingPortfolio);
            await context.SaveChangesAsync();

            // 將更新後的內容同步回原始物件
            portfolio.LastUpdated = existingPortfolio.LastUpdated;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新投資組合時發生錯誤。Portfolio ID: {PortfolioId}", portfolio.Id);
            throw;
        }
    }
    
    public async Task<bool> UpdateStockQuantityAsync(ObjectId portfolioId, ObjectId stockId, decimal quantity)
    {
        var portfolio = await context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == portfolioId);

        if (portfolio == null)
            return false;

        var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);
        if (stockEntry == null)
        {
            // 如果股票不存在，則新增
            portfolio.Stocks.Add(new PortfolioStock
            {
                StockId = stockId,
                Quantity = quantity
            });
        }
        else
        {
            // 更新現有股票數量
            stockEntry.Quantity = quantity;
        }

        portfolio.LastUpdated = DateTime.UtcNow;
        context.Portfolios.Update(portfolio);
        await context.SaveChangesAsync();

        return true;
    }
}
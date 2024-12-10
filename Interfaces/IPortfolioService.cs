namespace MinimalApiAot.Interfaces;

public interface IPortfolioService
{
    Task<IEnumerable<Portfolio>> GetAllAsync();
    Task<Portfolio?> GetByIdAsync(ObjectId id);
    Task<Portfolio?> GetByUserIdAsync(ObjectId userId);
    Task<Portfolio> CreateAsync(ObjectId userId);
    Task<bool> UpdateAsync(Portfolio portfolio);
    Task<bool> UpdateStockQuantityAsync(ObjectId portfolioId, ObjectId stockId, decimal quantity);
}
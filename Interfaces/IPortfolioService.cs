using Microsoft.EntityFrameworkCore.Storage;

namespace MinimalApiAot.Interfaces;

public interface IPortfolioService
{
    Task<IEnumerable<Portfolio>> GetAllAsync();
    Task<Portfolio?> GetByIdAsync(ObjectId id);
    Task<Portfolio?> GetByUserIdAsync(ObjectId userId);
    Task<Portfolio?> GetByUserNameAsync(string userName);
    Task<Portfolio> CreateAsync(ObjectId userId);
    Task<bool> UpdateAsync(Portfolio portfolio);
    void UpdatePortfolioStock(Portfolio portfolio, ObjectId stockId, decimal quantity);

}
namespace MinimalApiAot.Interfaces;

public interface IPortfolioDailyValueService
{
    Task<PortfolioDailyValueResponse?> GetPortfolioHistoryAsync(
        ObjectId portfolioId, 
        TimeRange range, 
        CancellationToken cancellationToken = default);
        
    Task<ValueSummary?> GetPortfolioSummaryAsync(
        ObjectId portfolioId, 
        TimeRange range, 
        CancellationToken cancellationToken = default);
}
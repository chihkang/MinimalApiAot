namespace MinimalApiAot.Services;

public class PortfolioDailyValueService(
    ApplicationDbContext dbContext,
    ILogger<PortfolioDailyValueService> logger)
    : IPortfolioDailyValueService
{
    public async Task<PortfolioDailyValueResponse?> GetPortfolioHistoryAsync(
        ObjectId portfolioId,
        TimeRange range,
        CancellationToken cancellationToken = default)
    {
        // 1. 先獲取日期範圍，避免無謂的資料查詢
        var dateRange = GetDateRange(range);

        // 2. 直接在查詢中加入日期過濾，減少記憶體使用
        var query = dbContext.PortfolioDailyValues
            .Where(p => p.PortfolioId == portfolioId)
            .Where(p => p.Date >= dateRange.StartDate && p.Date <= dateRange.EndDate)
            .OrderBy(p => p.Date);

        // 3. 直接映射到所需的資料結構，減少記憶體使用
        var dailyValues = await query
            .Select(x => new DailyValueData
            {
                Date = x.Date,
                TotalValueTwd = x.TotalValueTwd
            })
            .ToListAsync(cancellationToken);

        // 4. 使用更簡潔的日誌記錄
        logger.LogInformation(
            "Retrieved {Count} records for portfolio {PortfolioId} between {StartDate:yyyy-MM-dd} and {EndDate:yyyy-MM-dd}",
            dailyValues.Count,
            portfolioId,
            dateRange.StartDate,
            dateRange.EndDate);

        // 5. 提早返回，減少巢狀層級
        if (dailyValues.Count == 0)
        {
            logger.LogInformation("No data found for the specified criteria");
            return null;
        }

        // 6. 使用 AsReadOnly() 來確保集合不被修改
        var readOnlyDailyValues = dailyValues.AsReadOnly();
        var summary = ValueSummary.Calculate(readOnlyDailyValues);

        return new PortfolioDailyValueResponse(portfolioId, readOnlyDailyValues, summary);
    }

    public async Task<ValueSummary?> GetPortfolioSummaryAsync(
        ObjectId portfolioId,
        TimeRange range,
        CancellationToken cancellationToken = default)
    {
        var dateRange = GetDateRange(range);

        var query = dbContext.PortfolioDailyValues
            .Where(p => p.PortfolioId == portfolioId)
            .Where(p => p.Date >= dateRange.StartDate && p.Date <= dateRange.EndDate)
            .OrderBy(p => p.Date);

        var dailyValues = await query
            .Select(x => new DailyValueData
            {
                Date = x.Date,
                TotalValueTwd = x.TotalValueTwd
            })
            .ToListAsync(cancellationToken);

        if (dailyValues.Count == 0)
        {
            logger.LogInformation(
                "No summary data found for portfolio {PortfolioId} between {StartDate:yyyy-MM-dd} and {EndDate:yyyy-MM-dd}",
                portfolioId,
                dateRange.StartDate,
                dateRange.EndDate);
            return null;
        }

        var summary = ValueSummary.Calculate(dailyValues);

        logger.LogInformation(
            """
            Portfolio summary for {PortfolioId}:
            Start Value: {StartValue:N0}
            End Value: {EndValue:N0}
            Change: {ChangePercentage:N2}%
            """,
            portfolioId,
            summary.StartValue,
            summary.EndValue,
            summary.ChangePercentage);

        return summary;
    }

    private static DateTimeRange GetDateRange(TimeRange range)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = range switch
        {
            TimeRange.OneMonth => endDate.AddMonths(-1),
            TimeRange.ThreeMonths => endDate.AddMonths(-3),
            TimeRange.SixMonths => endDate.AddMonths(-6),
            TimeRange.OneYear => endDate.AddMonths(-12),
            _ => endDate.AddMonths(-1)
        };

        return new DateTimeRange(startDate, endDate);
    }
}
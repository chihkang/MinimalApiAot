namespace MinimalApiAot.Models.DTO;

public record DailyValueData
{
    public DateTime Date { get; init; }
    public decimal TotalValueTwd { get; init; }
}

public record PortfolioDailyValueResponse(
    ObjectId PortfolioId,
    IReadOnlyList<DailyValueData> Values,
    ValueSummary Summary);

public enum TimeRange
{
    OneMonth =1,
    ThreeMonths=3,
    SixMonths=6,
    OneYear=12
}
/// <summary>
/// 表示日期範圍的記錄類型
/// </summary>
public record DateTimeRange(DateTime StartDate, DateTime EndDate);
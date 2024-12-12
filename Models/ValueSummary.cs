namespace MinimalApiAot.Models;

public class ValueSummary
{
    public decimal StartValue { get; init; }
    public decimal EndValue { get; init; }
    public decimal ChangeAmount { get; init; }
    public decimal ChangePercentage { get; init; }
    public decimal HighestValue { get; init; }
    public DateTime HighestValueDate { get; init; }
    public decimal LowestValue { get; init; }
    public DateTime LowestValueDate { get; init; }
    
    public static ValueSummary Calculate(IReadOnlyList<DailyValueData> dailyValues)
    {
        if (!dailyValues.Any())
            throw new ArgumentException("Daily values cannot be empty", nameof(dailyValues));

        var startValue = dailyValues.First().TotalValueTwd;
        var endValue = dailyValues.Last().TotalValueTwd;
        var changeAmount = endValue - startValue;
        var changePercentage = startValue != 0 
            ? (changeAmount / startValue) * 100 
            : 0;

        var highestValueRecord = dailyValues.MaxBy(x => x.TotalValueTwd) ?? dailyValues[0];
        var lowestValueRecord = dailyValues.MinBy(x => x.TotalValueTwd) ?? dailyValues[0];


        return new ValueSummary
        {
            StartValue = startValue,
            EndValue = endValue,
            ChangeAmount = changeAmount,
            ChangePercentage = changePercentage,
            HighestValue = highestValueRecord.TotalValueTwd,
            HighestValueDate = highestValueRecord.Date,
            LowestValue = lowestValueRecord.TotalValueTwd,
            LowestValueDate = lowestValueRecord.Date
        };
    }
}
namespace MinimalApiAot.Data;

public class MongoDbContext(IMongoDatabase database)
{
    public IMongoCollection<User> Users { get; } = database.GetCollection<User>("users");
    public IMongoCollection<Stock> Stocks { get; } = database.GetCollection<Stock>("stocks");
    public IMongoCollection<Portfolio> Portfolios { get; } = database.GetCollection<Portfolio>("portfolio");
    public IMongoCollection<PortfolioDailyValue> PortfolioDailyValues { get; } = database.GetCollection<PortfolioDailyValue>("portfolio_daily_values");
    public IMongoCollection<PositionEvent> PositionEvents { get; } = database.GetCollection<PositionEvent>("positionEvents");
}

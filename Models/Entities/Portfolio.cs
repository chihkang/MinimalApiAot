namespace MinimalApiAot.Models.Entities;

public class Portfolio
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    [BsonElement("stocks")] public List<PortfolioStock> Stocks { get; set; } = new();

    [BsonElement("lastUpdated")] public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Version for optimistic concurrency control
    /// </summary>
    [BsonElement("version")]
    [ConcurrencyCheck]
    public long Version { get; set; }
    // 添加導航屬性
    [JsonIgnore]

    public virtual User? User { get; set; }
}

public class PortfolioStock
{
    [BsonElement("stockId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId StockId { get; set; }

    [BsonElement("quantity")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Quantity { get; set; }
}
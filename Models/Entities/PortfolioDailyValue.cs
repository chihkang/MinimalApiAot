namespace MinimalApiAot.Models.Entities;

public class PortfolioDailyValue
{
    [BsonId]
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    [BsonElement("PortfolioId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId PortfolioId { get; set; }
    [BsonElement("Date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; }
    [BsonElement("TotalValueTWD")]
    public decimal TotalValueTwd { get; set; }
}
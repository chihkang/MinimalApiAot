namespace MinimalApiAot.Models.Entities;

public class Stock
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    public required ObjectId Id { get; set; }
    
    [JsonPropertyName("name")]
    [BsonElement("name")]
    [BsonRequired]
    public required string Name { get; set; }
    
    [BsonElement("alias")]
    [JsonPropertyName("alias")]
    public required string Alias { get; set; }
    
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [BsonElement("currency")]
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    
    [BsonElement("lastUpdated")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
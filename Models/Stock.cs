namespace MinimalApiAot.Models;

public class Stock
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    [BsonElement("name")]
    [BsonRequired]
    public string Name { get; set; }
    
    [BsonElement("alias")]
    [JsonPropertyName("alias")]
    public string Alias { get; set; }
    
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [BsonElement("currency")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    
    [BsonElement("lastUpdated")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
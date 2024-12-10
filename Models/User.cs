namespace MinimalApiAot.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    public ObjectId Id { get; set; }

    [BsonElement("username")]
    [BsonRequired]
    [JsonPropertyName("username")] 
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    [BsonRequired]
    [JsonPropertyName("email")] 
    public string Email { get; set; } = string.Empty;

    [BsonElement("portfolioId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("portfolioId")]
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    public ObjectId PortfolioId { get; set; }

    [BsonElement("createdAt")]
    [BsonRepresentation(BsonType.DateTime)]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("settings")]
    [JsonPropertyName("settings")] 
    public UserSettings Settings { get; set; } = new();
}

public class UserSettings
{
    [BsonElement("currency")]
    [JsonPropertyName("currency")] 
    public string Currency { get; set; } = "TWD";

    [BsonElement("timeZone")]
    [JsonPropertyName("timeZone")] 
    public string TimeZone { get; set; } = "Asia/Taipei";
}
namespace MinimalApiAot.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    public string Id { get; set; } = null!;

    [BsonElement("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [BsonElement("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [BsonElement("portfolioId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("portfolioId")]
    public string PortfolioId { get; set; } = null!;

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("settings")]
    [JsonPropertyName("settings")]
    public UserSettings Settings { get; set; } = null!;
}

public class UserSettings
{
    [BsonElement("currency")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = null!;

    [BsonElement("timeZone")]
    [JsonPropertyName("timeZone")]
    public string TimeZone { get; set; } = null!;
}
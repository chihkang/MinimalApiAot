namespace MinimalApiAot.Models.DTO;

public record StockMinimalDto
{
    [JsonPropertyName("_id")]
    public string Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("alias")]
    public string Alias { get; init; }

    public StockMinimalDto(string id, string name, string alias)
    {
        Id = id;
        Name = name;
        Alias = alias;
    }
}
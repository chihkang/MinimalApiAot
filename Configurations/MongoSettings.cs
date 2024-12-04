namespace MinimalApiAot.Configurations;

public record MongoSettings
{
    public string ConnectionString { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
}
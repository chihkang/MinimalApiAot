namespace MinimalApiAot.Configurations;

public record MongoSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public bool UseSsl { get; set; }
    public bool AllowInsecureSsl { get; set; }
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 10;
    public int ConnectionTimeout { get; set; } = 10;
}
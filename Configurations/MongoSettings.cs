namespace MinimalApiAot.Configurations;

public record MongoSettings
{
    public const string SectionName = "MongoSettings";
    
    [Required(ErrorMessage = "MongoDB ConnectionString is required")]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "MongoDB DatabaseName is required")]
    public string DatabaseName { get; set; } = string.Empty;
    
    public bool UseSsl { get; set; } = true;
    public bool AllowInsecureSsl { get; set; } = false;
    
    [Range(10, 1000, ErrorMessage = "MaxConnectionPoolSize must be between 10 and 1000")]
    public int MaxConnectionPoolSize { get; set; } = 100;
    
    [Range(1, 100, ErrorMessage = "MinConnectionPoolSize must be between 1 and 100")]
    public int MinConnectionPoolSize { get; set; } = 10;
    
    [Range(1, 60, ErrorMessage = "ConnectionTimeout must be between 1 and 60 seconds")]
    public int ConnectionTimeout { get; set; } = 10;
}
namespace MinimalApiAot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection("MongoSettings"));
    
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
        
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new InvalidOperationException("MongoDB ConnectionString is not configured");
            }
        
            var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        
            // 簡化設定以減少 AOT 相關問題
            mongoSettings.MaxConnectionPoolSize = settings.MaxConnectionPoolSize;
            mongoSettings.MinConnectionPoolSize = settings.MinConnectionPoolSize;

            if (!settings.UseSsl) return new MongoClient(mongoSettings);
            mongoSettings.UseTls = true;
            mongoSettings.AllowInsecureTls = settings.AllowInsecureSsl;

            return new MongoClient(mongoSettings);
        });
        
        // 新增這個 IMongoDatabase 的註冊
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });
        
        return services;
    }
}
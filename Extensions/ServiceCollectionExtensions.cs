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
            
            // 創建 MongoDB 客戶端設定
            var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);

            // 配置連接池
            mongoSettings.MaxConnectionPoolSize = 100; // 最大連接數
            mongoSettings.MinConnectionPoolSize = 10; // 最小連接數
            mongoSettings.MaxConnectionIdleTime = TimeSpan.FromMinutes(1); // 連接最大閒置時間
            mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(10); // 連接超時時間
            mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5); // 服務器選擇超時

            // 配置重試策略
            mongoSettings.RetryWrites = true;
            mongoSettings.RetryReads = true;

            // 配置讀寫關注
            mongoSettings.WriteConcern = WriteConcern.WMajority;
            mongoSettings.ReadConcern = ReadConcern.Majority;

            // 可選：配置 SSL/TLS
            if (!settings.UseSsl) return new MongoClient(mongoSettings);
            mongoSettings.UseTls = true;
            mongoSettings.AllowInsecureTls = settings.AllowInsecureSsl; // 開發環境可能需要

            return new MongoClient(mongoSettings);
        });

        // 註冊 Repository
        services.AddScoped<IMongoRepository, MongoRepository>();

        return services;
    }
}
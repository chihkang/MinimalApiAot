
namespace MinimalApiAot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(
            configuration.GetSection("MongoSettings"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoRepository, MongoRepository>();
        
        return services;
    }

}
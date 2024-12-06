namespace MinimalApiAot.Extensions;

public static class MongoEndpointsExtension
{
    public static void MapMongoHealthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/mongodb")
            .WithTags("MongoDB Diagnostics");
            // 移除 .WithOpenApi()

        // 基本連接測試
        group.MapGet("/health", async (IMongoClient mongoClient, ILogger<IMongoClient> logger) =>
        {
            try
            {
                await mongoClient.ListDatabaseNamesAsync();
                logger.LogInformation("MongoDB health check successful");
                return Results.Ok(new MongoHealthResponse(
                    Status: "healthy",
                    Timestamp: DateTime.UtcNow,
                    Message: "MongoDB connection is successful"
                ));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MongoDB health check failed");
                return Results.Problem(
                    title: "MongoDB Connection Failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("MongoHealth")
        .WithDescription("Tests MongoDB connection health")
        .Produces<MongoHealthResponse>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // 詳細診斷資訊
        group.MapGet("/diagnostics", async (
            IMongoClient mongoClient,
            IOptions<MongoSettings> settings) =>
        {
            try
            {
                var serverInfo = await GetServerInfoAsync(mongoClient);
                var response = new MongoDiagnosticsResponse(
                    Status: "healthy",
                    Timestamp: DateTime.UtcNow,
                    ServerInfo: serverInfo,
                    Configuration: new MongoConfiguration(
                        settings.Value.DatabaseName,
                        settings.Value.MaxConnectionPoolSize,
                        settings.Value.MinConnectionPoolSize,
                        settings.Value.UseSsl
                    )
                );
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "MongoDB Diagnostics Failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("MongoDiagnostics")
        .WithDescription("Provides detailed MongoDB connection diagnostics")
        .Produces<MongoDiagnosticsResponse>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<MongoServerInfo> GetServerInfoAsync(IMongoClient mongoClient)
    {
        var serverStatus = await mongoClient.GetDatabase("admin")
            .RunCommandAsync<BsonDocument>(new BsonDocument { { "serverStatus", 1 } });

        return new MongoServerInfo(
            Version: serverStatus.GetValue("version", "Unknown").AsString,
            Uptime: serverStatus.GetValue("uptime", 0).ToInt32(),
            Connections: new MongoConnections(
                Current: serverStatus["connections"]?["current"]?.AsInt32 ?? 0,
                Available: serverStatus["connections"]?["available"]?.AsInt32 ?? 0,
                TotalCreated: serverStatus["connections"]?["totalCreated"]?.AsInt32 ?? 0
            )
        );
    }
}

// 新增回應模型
public record MongoHealthResponse(
    string Status,
    DateTime Timestamp,
    string Message
);

public record MongoDiagnosticsResponse(
    string Status,
    DateTime Timestamp,
    MongoServerInfo ServerInfo,
    MongoConfiguration Configuration
);

public record MongoServerInfo(
    string Version,
    int Uptime,
    MongoConnections Connections
);

public record MongoConnections(
    int Current,
    int Available,
    int TotalCreated
);

public record MongoConfiguration(
    string Database,
    int MaxPoolSize,
    int MinPoolSize,
    bool UseSsl
);
// 註冊 MongoDB 序列化器

BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
BsonSerializer.RegisterSerializer(typeof(decimal?),
    new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
BsonSerializer.RegisterSerializer(typeof(ObjectId), new ObjectIdSerializer(BsonType.ObjectId));
BsonSerializer.RegisterSerializer(typeof(ObjectId?),
    new NullableSerializer<ObjectId>(new ObjectIdSerializer(BsonType.ObjectId)));
BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Utc));
BsonSerializer.RegisterSerializer(typeof(DateTime?),
    new NullableSerializer<DateTime>(new DateTimeSerializer(DateTimeKind.Utc)));
BsonSerializer.RegisterSerializer(typeof(PositionEventType), new EnumSerializer<PositionEventType>(BsonType.String));
BsonSerializer.RegisterSerializer(typeof(Currency), new EnumSerializer<Currency>(BsonType.String));
BsonSerializer.RegisterSerializer(typeof(string), new StringSerializer(BsonType.String));

BsonSerializer.RegisterSerializer(typeof(long), new Int64Serializer(BsonType.Int64));
BsonSerializer.RegisterSerializer(typeof(long?),
    new NullableSerializer<long>(new Int64Serializer(BsonType.Int64)));

RegisterMongoClassMaps();
BsonSerializer.RegisterSerializer(
    typeof(List<PortfolioStock>),
    new PortfolioStockListSerializer());

BsonDefaults.DynamicDocumentSerializer = new BsonDocumentSerializer();
BsonDefaults.DynamicArraySerializer = new BsonArraySerializer();

static void RegisterMongoClassMaps()
{
    if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
    {
        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(UserSettings)))
    {
        BsonClassMap.RegisterClassMap<UserSettings>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(Stock)))
    {
        BsonClassMap.RegisterClassMap<Stock>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(PortfolioStock)))
    {
        BsonClassMap.RegisterClassMap<PortfolioStock>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(Portfolio)))
    {
        BsonClassMap.RegisterClassMap<Portfolio>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(p => p.Stocks).SetSerializer(new PortfolioStockListSerializer());
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(PortfolioDailyValue)))
    {
        BsonClassMap.RegisterClassMap<PortfolioDailyValue>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }

    if (!BsonClassMap.IsClassMapRegistered(typeof(PositionEvent)))
    {
        BsonClassMap.RegisterClassMap<PositionEvent>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }
}

var builder = WebApplication.CreateBuilder(args);

static string EnsureMongoTlsOptions(string connectionString, bool useTls, bool allowInsecureTls)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        return connectionString;

    var parts = connectionString.Split('?', 2);
    var basePart = parts[0];
    var query = parts.Length > 1 ? parts[1] : string.Empty;

    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var segment in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
        var kv = segment.Split('=', 2);
        options[kv[0]] = kv.Length == 2 ? kv[1] : string.Empty;
    }

    var tls = useTls ? "true" : "false";
    options["tls"] = tls;
    options["ssl"] = tls;

    var insecure = allowInsecureTls ? "true" : "false";
    options["tlsInsecure"] = insecure;
    options["tlsAllowInvalidCertificates"] = insecure;
    options["tlsAllowInvalidHostnames"] = insecure;

    var newQuery = string.Join("&", options.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    return newQuery.Length == 0 ? basePart : $"{basePart}?{newQuery}";
}

// 2. 確保配置來源正確設定
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() // 添加環境變數支援
    .Build();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddOptions<MongoSettings>()
    .Bind(builder.Configuration.GetSection("MongoSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>()
                    ?? throw new InvalidOperationException("MongoSettings section is required.");

// 對於 mongodb+srv:// 協議，TLS 已自動啟用，不需要額外檢查
var isSrvProtocol = mongoSettings.ConnectionString.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase);
if (!builder.Environment.IsDevelopment() && !isSrvProtocol && (!mongoSettings.UseSsl || mongoSettings.AllowInsecureSsl))
    throw new InvalidOperationException("In production, MongoDB TLS must be enabled and insecure TLS must be disabled.");

// 1. 首先註冊 MongoClient 為單例
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    try
    {
        // 對於 mongodb+srv:// 協議，直接使用連接字串，不修改 TLS 選項
        var connectionString = isSrvProtocol
            ? mongoSettings.ConnectionString
            : EnsureMongoTlsOptions(
                mongoSettings.ConnectionString,
                useTls: mongoSettings.UseSsl,
                allowInsecureTls: mongoSettings.AllowInsecureSsl);

        return new MongoClient(connectionString);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to initialize MongoDB connection", ex);
    }
});
builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(mongoSettings.DatabaseName);
});
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddHealthChecks()
    .AddMongoDb(
        serviceProvider => serviceProvider.GetRequiredService<IMongoClient>(),
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3));
// 2. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
// 註冊 UserService
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IPortfolioDailyValueService, PortfolioDailyValueService>();
builder.Services.AddScoped<IPositionEventService, PositionEventService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
    // 控制 JSON 輸出的格式是否要進行縮排（美化）
    options.SerializerOptions.WriteIndented = false;
    // 序列化時會忽略所有值為 null 的屬性
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new ObjectIdJsonConverter());
});
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
var app = builder.Build();

EnsureMongoIndexes(app.Services.GetRequiredService<MongoDbContext>());

// 在 Production 環境（如 Zeabur）不使用 HTTPS Redirect，因為平台已經處理了 HTTPS
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapGet("/", () => "Hello World!");
app.MapGet("/time", () => DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));

// Register endpoints
app.MapUserEndpoints();
app.MapStockEndpoints();
app.MapPortfolioEndpoints();
app.MapPortfolioDailyValueEndpoints();
app.MapPositionEventEndpoints();

app.Run();

static void EnsureMongoIndexes(MongoDbContext db)
{
    var positionEventIndexes = new List<CreateIndexModel<PositionEvent>>
    {
        new(
            Builders<PositionEvent>.IndexKeys.Ascending(e => e.OperationId),
            new CreateIndexOptions { Unique = true, Name = "ux_operationId" }),
        new(
            Builders<PositionEvent>.IndexKeys.Ascending(e => e.UserId).Descending(e => e.TradeAt),
            new CreateIndexOptions { Name = "ix_user_tradeAt" }),
        new(
            Builders<PositionEvent>.IndexKeys.Ascending(e => e.StockId).Descending(e => e.TradeAt),
            new CreateIndexOptions { Name = "ix_stock_tradeAt" })
    };

    db.PositionEvents.Indexes.CreateMany(positionEventIndexes);
}
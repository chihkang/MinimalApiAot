// 註冊 MongoDB 序列化器

using DotNetEnv;

BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
BsonSerializer.RegisterSerializer(typeof(decimal?),
    new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

var builder = WebApplication.CreateBuilder(args);

// 載入 .env 檔案
Env.Load();

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

var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>()
                    ?? throw new InvalidOperationException("MongoSettings section is required.");

if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
    throw new InvalidOperationException("MongoSettings:ConnectionString is required (set env var MongoSettings__ConnectionString).");

if (string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
    throw new InvalidOperationException("MongoSettings:DatabaseName is required (set env var MongoSettings__DatabaseName).");

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

// 2. 修改 DbContext 的註冊方式
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    options.UseMongoDB(mongoClient, mongoSettings.DatabaseName);
    
    // 加入警告配置
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
});
// 2. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// 在 Production 環境（如 Zeabur）不使用 HTTPS Redirect，因為平台已經處理了 HTTPS
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");
app.MapGet("/time", () => DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));

// Register endpoints
app.MapUserEndpoints();
app.MapStockEndpoints();
app.MapPortfolioEndpoints();
app.MapPortfolioDailyValueEndpoints();
app.MapPositionEventEndpoints();

app.Run();
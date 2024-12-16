// 註冊 MongoDB 序列化器
BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
BsonSerializer.RegisterSerializer(typeof(decimal?),
    new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

var builder = WebApplication.CreateBuilder(args);

// 1. 驗證配置
var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
// 1. 首先註冊 MongoClient 為單例
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    try
    {
        return new MongoClient(mongoSettings?.ConnectionString);
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

app.UseHttpsRedirection();

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

app.Run();
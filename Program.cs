// 註冊 MongoDB 序列化器

BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
BsonSerializer.RegisterSerializer(typeof(decimal?),
    new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

var builder = WebApplication.CreateBuilder(args);

// 1. 驗證配置
var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
if (mongoSettings == null || string.IsNullOrEmpty(mongoSettings.ConnectionString))
{
    throw new InvalidOperationException("MongoDB settings are not properly configured");
}

// 註冊 MongoDB EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    try
    {
        var mongoClient = new MongoClient(mongoSettings.ConnectionString);
        options.UseMongoDB(mongoClient, mongoSettings.DatabaseName);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to initialize MongoDB connection", ex);
    }
});
// 2. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 註冊 UserService
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStockService, StockService>();
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

app.Run();
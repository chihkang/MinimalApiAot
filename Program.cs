var builder = WebApplication.CreateBuilder(args);

// 1. 驗證配置
var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
if (mongoSettings == null || string.IsNullOrEmpty(mongoSettings.ConnectionString))
{
    throw new InvalidOperationException("MongoDB settings are not properly configured");
}

// 2. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
    // 控制 JSON 輸出的格式是否要進行縮排（美化）
    options.SerializerOptions.WriteIndented = false;
    // 序列化時會忽略所有值為 null 的屬性
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
app.MapItemEndpoints();
app.MapMongoHealthEndpoints();

app.Run();
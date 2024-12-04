
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 2. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
});
var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => "Hello World!");
app.MapGet("/time", () => DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));

app.UseHttpsRedirection();

// Register endpoints
app.MapItemEndpoints();

app.Run();

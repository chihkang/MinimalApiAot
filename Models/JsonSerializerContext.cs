using MinimalApiAot.Models.Entities;

namespace MinimalApiAot.Models;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(OpenApiInfo))]
[JsonSerializable(typeof(OpenApiDocument))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(UserSettings))]
[JsonSerializable(typeof(ObjectId))]
[JsonSerializable(typeof(IEnumerable<StockMinimalDto>))]
[JsonSerializable(typeof(UpdateStockPriceResponse))]
[JsonSerializable(typeof(Stock))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
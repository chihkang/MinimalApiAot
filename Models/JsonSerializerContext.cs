namespace MinimalApiAot.Models;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(MongoHealthResponse))]  // 添加我們的響應類型
[JsonSerializable(typeof(MongoDiagnosticsResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(Item))]  // 單個 Item
[JsonSerializable(typeof(List<Item>))]  // Item 列表
[JsonSerializable(typeof(IEnumerable<Item>))]  // IEnumerable<Item>
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(OpenApiInfo))]  // 添加 OpenAPI 相關類型
[JsonSerializable(typeof(OpenApiDocument))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
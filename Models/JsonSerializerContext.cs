namespace MinimalApiAot.Models;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(MongoHealthResponse))]
[JsonSerializable(typeof(MongoDiagnosticsResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(List<Item>))]
[JsonSerializable(typeof(IEnumerable<Item>))]
[JsonSerializable(typeof(OpenApiInfo))]
[JsonSerializable(typeof(OpenApiDocument))]
[JsonSerializable(typeof(MongoServerInfo))]
[JsonSerializable(typeof(MongoConnections))]
[JsonSerializable(typeof(MongoConfiguration))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(UserSettings))]
[JsonSerializable(typeof(ObjectId))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
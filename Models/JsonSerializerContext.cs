
namespace MinimalApiAot.Models;

[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(IEnumerable<Item>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
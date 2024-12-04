namespace MinimalApiAot.Models;

public record Item
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
}
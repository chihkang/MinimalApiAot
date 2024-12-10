namespace MinimalApiAot.Models.DTO;

public record CreateUserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public UserSettings? Settings { get; init; }
}
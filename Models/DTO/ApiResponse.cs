namespace MinimalApiAot.Models.DTO;

public class ApiResponse<T>(string message, T? data = default)
{
    public string Message { get; set; } = message;
    public T? Data { get; set; } = data;
}
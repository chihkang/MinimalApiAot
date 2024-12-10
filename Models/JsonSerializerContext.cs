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
[JsonSerializable(typeof(Portfolio))]
[JsonSerializable(typeof(List<Portfolio>))]
[JsonSerializable(typeof(IEnumerable<Portfolio>))]
[JsonSerializable(typeof(PortfolioStock))]
[JsonSerializable(typeof(PortfolioResponseDto))]
[JsonSerializable(typeof(List<PortfolioResponseDto>))]
[JsonSerializable(typeof(UpdatePortfolioStockDto))]
[JsonSerializable(typeof(List<UpdatePortfolioStockDto>))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(UpdateStockQuantityRequest))]
[JsonSerializable(typeof(UpdateStockByIdRequest))]
[JsonSerializable(typeof(UpdateStockByNameRequest))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
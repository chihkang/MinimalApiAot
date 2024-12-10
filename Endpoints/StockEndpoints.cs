namespace MinimalApiAot.Endpoints;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stocks")
            .WithTags("Stocks")
            .WithOpenApi();

        group.MapGet("/minimal", GetAllStocksMinimal)
            .WithName("GetAllStocksMinimal")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get all stocks with minimal information (ID, Name, and Alias)",
                Description = "返回所有股票的基本資訊，包含代碼、名稱和別名"
            });

        group.MapPut("/{name}/price", UpdateStockPrice)
            .WithName("UpdateStockPrice")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update stock price by name (e.g., \"2330:TPE\") with optimized performance",
                Description = "根據股票代碼更新股票價格，並返回更新前後的價格資訊"
            });
    }

    private static async Task<IResult> GetAllStocksMinimal(IStockService stockService)
    {
        try
        {
            var stocks = await stockService.GetAllStocksMinimalAsync();
            return Results.Ok(stocks);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "無法獲取股票資訊",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateStockPrice(
        string name,
        decimal newPrice,
        IStockService stockService)
    {
        try
        {
            if (newPrice <= 0)
            {
                return Results.BadRequest(new { message = "股票價格必須大於0" });
            }

            var response = await stockService.UpdateStockPriceAsync(name, newPrice);

            return response != null
                ? Results.Ok(response)
                : Results.NotFound(new { message = $"找不到股票代碼: {name}" });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "更新股票價格失敗",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
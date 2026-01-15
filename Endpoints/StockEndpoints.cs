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
        group.MapPut("/id/{stockId}/price", UpdateStockPriceById)
            .WithName("UpdateStockPriceByID")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update stock price by ID (e.g., \"67283b36447a55a757f87daf\") with optimized performance",
                Description = "根據股票ID更新股票價格，並返回更新前後的價格資訊"
            });

        group.MapPut("/batch-price", UpdateStockPricesBatch)
            .WithName("UpdateStockPricesBatch")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update multiple stock prices in batch (Max 20)",
                Description = "批次更新多支股票價格，單次上限 20 筆。採用部分成功策略，會回傳成功、找不到 ID 以及無效 ID 的清單。"
            });
    }

    private static async Task<IResult> UpdateStockPricesBatch(BatchUpdateStockPriceRequest request,
        IStockService stockService)
    {
        if (request.Updates == null || request.Updates.Count == 0)
        {
            return Results.BadRequest(ErrorResponse.Create("更新清單不可為空"));
        }

        if (request.Updates.Count > 20)
        {
            return Results.BadRequest(ErrorResponse.Create("單次批次更新上限為 20 筆"));
        }

        if (request.Updates.Any(u => u.NewPrice <= 0))
        {
            return Results.BadRequest(ErrorResponse.Create("所有股票價格必須大於 0"));
        }

        var response = await stockService.UpdateStockPricesBatchAsync(request.Updates);
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateStockPriceById(ObjectId stockId,
        decimal newPrice,
        IStockService stockService)
    {
        try
        {
            if (newPrice <= 0)
            {
                return Results.BadRequest(ErrorResponse.Create("股票價格必須大於0"));
            }

            var response = await stockService.UpdateStockPriceAsync(stockId, newPrice);

            return response != null
                ? Results.Ok(response)
                : Results.NotFound(ErrorResponse.Create($"找不到股票: {stockId}"));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "更新股票價格失敗",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
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
                return Results.BadRequest(ErrorResponse.Create("股票價格必須大於0"));
            }

            var response = await stockService.UpdateStockPriceAsync(name, newPrice);

            return response != null
                ? Results.Ok(response)
                : Results.NotFound(ErrorResponse.Create($"找不到股票代碼: {name}"));
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
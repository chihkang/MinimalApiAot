namespace MinimalApiAot.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolios")
            .WithTags("Portfolios");

        group.MapGet("/", GetAllPortfolios)
            .WithName("GetAllPortfolios")
            .WithDescription("Get all portfolios");

        group.MapGet("/{id}", GetPortfolioById)
            .WithName("GetPortfolioById")
            .WithDescription("Get a portfolio by id");

        group.MapGet("/user/{userId}", GetPortfolioByUserId)
            .WithName("GetPortfolioByUserId")
            .WithDescription("Get a portfolio by user id");

        group.MapGet("/user/name/{userName}", GetPortfolioByUserName)
            .WithName("GetPortfolioByUserName")
            .WithDescription("Get a portfolio by user name");

        // 更新投資組合中特定股票的數量
        group.MapPut("/{id}/stocks/{stockId}", UpdatePortfolioStockQuantity)
            .WithName("UpdatePortfolioStockQuantity")
            .WithDescription("更新投資組合中特定股票的數量");

        group.MapDelete("/{id}/stocks/{stockId}", RemoveStockFromPortfolio)
            .WithName("RemoveStockFromPortfolio")
            .WithDescription("Remove a stock from portfolio");

        group.MapPut("/{id}/stocks", UpdatePortfolioStockByName)
            .WithName("UpdatePortfolioStockByName")
            .WithDescription("根據股票名稱或代號更新投資組合中的股票數量");
    }

    private static async Task<IResult> GetPortfolioByUserName(IPortfolioService portfolioService, string userName)
    {
        var portfolio = await portfolioService.GetByUserNameAsync(userName);
        return portfolio is null ? TypedResults.NotFound() : TypedResults.Ok(portfolio);
    }

    private static async Task<IResult> GetAllPortfolios(
        IPortfolioService portfolioService)
    {
        var portfolios = await portfolioService.GetAllAsync();
        return TypedResults.Ok(portfolios);
    }

    private static async Task<IResult> GetPortfolioById(
        IPortfolioService portfolioService,
        ObjectId id)
    {
        var portfolio = await portfolioService.GetByIdAsync(id);
        return portfolio is null ? TypedResults.NotFound() : TypedResults.Ok(portfolio);
    }

    private static async Task<IResult> GetPortfolioByUserId(
        IPortfolioService portfolioService,
        ObjectId userId)
    {
        var portfolio = await portfolioService.GetByUserIdAsync(userId);
        return portfolio is null ? TypedResults.NotFound() : TypedResults.Ok(portfolio);
    }

    private static async Task<IResult> UpdatePortfolioStockQuantity(
        IPortfolioService portfolioService,
        ObjectId id,
        ObjectId stockId,
        UpdateStockQuantityRequest request)
    {
        try
        {
            var portfolio = await portfolioService.GetByIdAsync(id);
            if (portfolio is null)
                return TypedResults.NotFound("找不到指定的投資組合");

            var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);
            if (stockEntry is null)
            {
                // 如果股票不存在於投資組合中，新增該股票
                portfolio.Stocks.Add(new PortfolioStock
                {
                    StockId = stockId,
                    Quantity = request.Quantity
                });
            }
            else
            {
                // 更新既有股票的數量
                stockEntry.Quantity = request.Quantity;
            }

            var updateResult = await portfolioService.UpdateAsync(portfolio);
            if (!updateResult)
                return TypedResults.Problem("更新投資組合失敗");

            return TypedResults.Ok(portfolio);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ApiResponse<string>(ex.Message));
        }
    }

    private static async Task<IResult> RemoveStockFromPortfolio(
        IPortfolioService portfolioService,
        ObjectId id,
        ObjectId stockId)
    {
        var portfolio = await portfolioService.GetByIdAsync(id);
        if (portfolio is null) return TypedResults.NotFound();

        var stock = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);
        if (stock is null) return TypedResults.NotFound();

        portfolio.Stocks.Remove(stock);
        await portfolioService.UpdateAsync(portfolio);

        return TypedResults.Ok(portfolio);
    }

    private static async Task<IResult> UpdatePortfolioStockByName(
        IPortfolioService portfolioService,
        IStockService stockService,
        ObjectId id,
        UpdateStockByNameRequest request)
    {
        try
        {
            // 驗證投資組合
            var portfolio = await portfolioService.GetByIdAsync(id);
            if (portfolio is null)
            {
                return TypedResults.NotFound(new ApiResponse<string>("找不到指定的投資組合"));
            }

            // 查詢股票
            var stock = await stockService.GetByNameOrAliasAsync(request.StockNameOrAlias);
            if (stock is null)
            {
                return TypedResults.NotFound(new ApiResponse<string>("找不到指定的股票"));
            }

            portfolioService.UpdatePortfolioStock(portfolio, stock.Id, request.Quantity);

            // 更新投資組合
            var updateResult = await portfolioService.UpdateAsync(portfolio);
            if (!updateResult)
            {
                return TypedResults.UnprocessableEntity(new ApiResponse<string>("更新投資組合失敗"));
            }


            return TypedResults.Ok(new ApiResponse<Portfolio>("更新成功", portfolio));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new ApiResponse<string>(ex.Message));
        }
    }
}
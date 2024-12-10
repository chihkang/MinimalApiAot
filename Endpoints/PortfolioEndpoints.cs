namespace MinimalApiAot.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolios")
            .WithTags("Portfolios")
            .WithOpenApi();

        group.MapGet("/", GetAllPortfolios)
            .WithName("GetAllPortfolios")
            .WithDescription("Get all portfolios");

        group.MapGet("/{id}", GetPortfolioById)
            .WithName("GetPortfolioById")
            .WithDescription("Get a portfolio by id");

        group.MapGet("/user/{userId}", GetPortfolioByUserId)
            .WithName("GetPortfolioByUserId")
            .WithDescription("Get a portfolio by user id");

        // 更新投資組合中特定股票的數量
        group.MapPut("/{id}/stocks/{stockId}", UpdatePortfolioStockQuantity)
            .WithName("UpdatePortfolioStockQuantity")
            .WithDescription("更新投資組合中特定股票的數量");

        group.MapDelete("/{id}/stocks/{stockId}", RemoveStockFromPortfolio)
            .WithName("RemoveStockFromPortfolio")
            .WithDescription("Remove a stock from portfolio");

        // 根據股票 ID 更新投資組合股票
        group.MapPut("/{id}/stocks/byId", UpdatePortfolioStockById)
            .WithName("UpdatePortfolioStockById")
            .WithDescription("根據股票 ID 更新投資組合中的股票數量");

        group.MapPut("/{id}/stocks", UpdatePortfolioStockByName)
            .WithName("UpdatePortfolioStockByName")
            .WithDescription("根據股票名稱或代號更新投資組合中的股票數量")
            .WithOpenApi();
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
            return TypedResults.BadRequest(new { error = ex.Message });
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

    private static async Task<IResult> UpdatePortfolioStockById(
        IPortfolioService portfolioService,
        IStockService stockService,
        ObjectId portfolioId,
        ObjectId stockId,
        UpdateStockByIdRequest request)
    {
        try
        {
            var portfolio = await portfolioService.GetByIdAsync(portfolioId);
            if (portfolio is null)
                return TypedResults.NotFound("找不到指定的投資組合");

            var stock = await stockService.GetByIdAsync(stockId);
            if (stock is null)
                return TypedResults.NotFound("找不到指定的股票");

            var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);
            if (stockEntry is null)
            {
                portfolio.Stocks.Add(new PortfolioStock
                {
                    StockId = stockId,
                    Quantity = request.Quantity
                });
            }
            else
            {
                stockEntry.Quantity = request.Quantity;
            }

            var updateResult = await portfolioService.UpdateAsync(portfolio);
            if (!updateResult)
                return TypedResults.Problem("更新投資組合失敗");

            return TypedResults.Ok(portfolio);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
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
                return TypedResults.NotFound(new { message = "找不到指定的投資組合" });

            // 查詢股票
            var stock = await stockService.GetByNameOrAliasAsync(request.StockNameOrAlias);
            if (stock is null)
                return TypedResults.NotFound(new { message = "找不到指定的股票" });

            await UpdatePortfolioStock(portfolio, stock.Id, request.Quantity);

            // 更新投資組合
            var updateResult = await portfolioService.UpdateAsync(portfolio);
            if (!updateResult)
                return TypedResults.UnprocessableEntity(new { message = "更新投資組合失敗" });

            return TypedResults.Ok(new
            {
                message = "更新成功",
                portfolio = portfolio
            });
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { message = ex.Message });
        }
    }

    // 抽取更新股票邏輯為獨立方法
    private static Task UpdatePortfolioStock(Portfolio portfolio, ObjectId stockId, decimal quantity)
    {
        var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);

        if (stockEntry is null)
        {
            portfolio.Stocks.Add(new PortfolioStock
            {
                StockId = stockId,
                Quantity = quantity
            });
        }
        else
        {
            stockEntry.Quantity = quantity;
        }

        return Task.CompletedTask;
    }
}
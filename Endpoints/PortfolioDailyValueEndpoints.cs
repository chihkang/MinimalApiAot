namespace MinimalApiAot.Endpoints;

public static class PortfolioDailyValueEndpoints
{
    public static void MapPortfolioDailyValueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolioDailyValue")
            .WithTags("portfolioDailyValue")
            .WithOpenApi();

        group.MapGet("/{portfolioId}/history", GetAllPortfolioHistory)
            .WithName("GetPortfolioHistory")
            .WithOpenApi()
            .WithDescription("Get portfolio daily value history")
            .WithDisplayName("Get portfolio history");

        group.MapGet("/{portfolioId}/summary", GetPortfolioSummary)
            .WithName("GetPortfolioSummary")
            .WithOpenApi()
            .WithDescription("Get portfolio summary")
            .WithDisplayName("Get portfolio summary");
    }

    private static async Task<IResult> GetPortfolioSummary(IPortfolioDailyValueService portfolioDailyValueService,
        string portfolioId,
        [FromQuery] TimeRange range = TimeRange.OneMonth,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(portfolioId, out var id))
        {
            return Results.BadRequest("Invalid portfolio ID format");
        }

        try
        {
            var result = await portfolioDailyValueService.GetPortfolioSummaryAsync(
                id,
                range,
                cancellationToken);

            return result is null ? Results.NotFound($"No data found for portfolio {portfolioId}") : Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while retrieving portfolio summary");
        }
    }

    private static async Task<IResult> GetAllPortfolioHistory(IPortfolioDailyValueService portfolioDailyValueService,
        string portfolioId,
        [FromQuery] TimeRange range = TimeRange.OneMonth,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(portfolioId, out var id))
        {
            return Results.BadRequest("Invalid portfolio ID format");
        }

        try
        {
            var result = await portfolioDailyValueService.GetPortfolioHistoryAsync(
                id,
                range,
                cancellationToken);

            return result is null ? Results.NotFound() : Results.Ok(result);
        }
        catch (Exception)
        {
            return Results.Problem("An error occurred while retrieving portfolio history");
        }
    }
}
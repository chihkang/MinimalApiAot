namespace MinimalApiAot.Endpoints;

public static class PositionEventEndpoints
{
    public static void MapPositionEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/positionevents")
            .WithTags("PositionEvents");

        // GET all position events (paginated)
        group.MapGet("/", GetAll)
            .WithName("GetAllPositionEvents")
            .WithSummary("Get all position events with pagination")
            .WithDescription("取得所有持倉異動紀錄，支援分頁");

        // GET position event by ID
        group.MapGet("/{id}", GetById)
            .WithName("GetPositionEventById")
            .WithSummary("Get position event by ID")
            .WithDescription("根據 ID 取得單筆持倉異動紀錄");

        // GET position event by operationId
        group.MapGet("/operation/{operationId}", GetByOperationId)
            .WithName("GetPositionEventByOperationId")
            .WithSummary("Get position event by operation ID")
            .WithDescription("根據 operationId (UUID) 取得持倉異動紀錄");

        // GET position events by user ID (paginated, filtered)
        group.MapGet("/user/{userId}", GetByUserId)
            .WithName("GetPositionEventsByUserId")
            .WithSummary("Get position events by user ID with filtering")
            .WithDescription("取得指定使用者的持倉異動紀錄，支援按類型、日期範圍篩選，預設查詢最近一年，支援分頁");

        // GET position events by stock ID (paginated, filtered)
        group.MapGet("/stock/{stockId}", GetByStockId)
            .WithName("GetPositionEventsByStockId")
            .WithSummary("Get position events by stock ID with filtering")
            .WithDescription("取得指定股票的持倉異動紀錄，支援按類型、日期範圍篩選，預設查詢最近一年，支援分頁");

        // POST create position event
        group.MapPost("/", Create)
            .WithName("CreatePositionEvent")
            .WithSummary("Create a new position event")
            .WithDescription("建立新的持倉異動紀錄（BUY/SELL/REPLACE），同時同步更新 Portfolio 持倉數量。會驗證 iOS 傳入的計算欄位。");

        // PUT update position event
        group.MapPut("/{id}", Update)
            .WithName("UpdatePositionEvent")
            .WithSummary("Update an existing position event")
            .WithDescription("更新現有持倉異動紀錄（用於錯誤修正），同時同步更新 Portfolio 持倉數量");

        // DELETE position event
        group.MapDelete("/{id}", Delete)
            .WithName("DeletePositionEvent")
            .WithSummary("Delete a position event")
            .WithDescription("刪除持倉異動紀錄，同時回滾 Portfolio 持倉數量至異動前狀態");
    }

    #region Endpoint Handlers

    private static async Task<IResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IPositionEventService positionEventService = default!,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await positionEventService.GetAllAsync(pagination, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        string id,
        IPositionEventService positionEventService,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return Results.BadRequest(ErrorResponse.Create("Invalid ID format"));
        }

        var positionEvent = await positionEventService.GetByIdAsync(objectId, cancellationToken);

        return positionEvent != null
            ? Results.Ok(PositionEventResponseDto.FromEntity(positionEvent))
            : Results.NotFound(ErrorResponse.Create($"PositionEvent with ID {id} not found"));
    }

    private static async Task<IResult> GetByOperationId(
        string operationId,
        IPositionEventService positionEventService,
        CancellationToken cancellationToken = default)
    {
        var positionEvent = await positionEventService.GetByOperationIdAsync(operationId, cancellationToken);

        return positionEvent != null
            ? Results.Ok(PositionEventResponseDto.FromEntity(positionEvent))
            : Results.NotFound(ErrorResponse.Create($"PositionEvent with operationId {operationId} not found"));
    }

    private static async Task<IResult> GetByUserId(
        string userId,
        [FromQuery] PositionEventType? type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IPositionEventService positionEventService = default!,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(userId, out var userObjectId))
        {
            return Results.BadRequest(ErrorResponse.Create("Invalid userId format"));
        }

        var query = new PositionEventQueryRequest
        {
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await positionEventService.GetByUserIdAsync(userObjectId, query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByStockId(
        string stockId,
        [FromQuery] PositionEventType? type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        IPositionEventService positionEventService = default!,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(stockId, out var stockObjectId))
        {
            return Results.BadRequest(ErrorResponse.Create("Invalid stockId format"));
        }

        var query = new PositionEventQueryRequest
        {
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await positionEventService.GetByStockIdAsync(stockObjectId, query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreatePositionEventRequest request,
        IPositionEventService positionEventService,
        ILogger<PositionEventEndpointsLogger> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await positionEventService.CreateAsync(request, cancellationToken);

            if (result.Success)
            {
                var response = PositionEventResponseDto.FromEntity(result.PositionEvent!);
                return Results.Created($"/api/positionevents/{result.PositionEvent!.Id}", response);
            }

            return result.ErrorType switch
            {
                PositionEventErrorType.DuplicateOperationId => Results.Conflict(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                PositionEventErrorType.ConcurrencyConflict => Results.Conflict(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                PositionEventErrorType.NotFound or
                PositionEventErrorType.UserNotFound or
                PositionEventErrorType.StockNotFound or
                PositionEventErrorType.PortfolioNotFound => Results.NotFound(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                PositionEventErrorType.ValidationFailed => Results.BadRequest(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                _ => Results.Problem(
                    title: "Failed to create position event",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status500InternalServerError)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating position event");
            return Results.Problem(
                title: "Failed to create position event",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> Update(
        string id,
        UpdatePositionEventRequest request,
        IPositionEventService positionEventService,
        ILogger<PositionEventEndpointsLogger> logger,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return Results.BadRequest(ErrorResponse.Create("Invalid ID format"));
        }

        try
        {
            var result = await positionEventService.UpdateAsync(objectId, request, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(PositionEventResponseDto.FromEntity(result.PositionEvent!));
            }

            return result.ErrorType switch
            {
                PositionEventErrorType.NotFound => Results.NotFound(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                PositionEventErrorType.ConcurrencyConflict => Results.Conflict(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                PositionEventErrorType.ValidationFailed => Results.BadRequest(
                    ErrorResponse.Create(result.ErrorMessage!, result.ErrorType.ToString())
                ),
                _ => Results.Problem(
                    title: "Failed to update position event",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status500InternalServerError)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating position event {Id}", id);
            return Results.Problem(
                title: "Failed to update position event",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> Delete(
        string id,
        IPositionEventService positionEventService,
        ILogger<PositionEventEndpointsLogger> logger,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return Results.BadRequest(ErrorResponse.Create("Invalid ID format"));
        }

        try
        {
            var result = await positionEventService.DeleteAsync(objectId, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(SuccessResponse.Create("Position event deleted and portfolio rolled back successfully"));
            }

            return result.ErrorType switch
            {
                PositionEventErrorType.NotFound => Results.NotFound(ErrorResponse.Create(result.ErrorMessage, result.ErrorType.ToString())),
                PositionEventErrorType.ConcurrencyConflict => Results.Conflict(ErrorResponse.Create(result.ErrorMessage, result.ErrorType.ToString())),
                _ => Results.Problem(
                    title: "Failed to delete position event",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status500InternalServerError)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting position event {Id}", id);
            return Results.Problem(
                title: "Failed to delete position event",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    #endregion
}

/// <summary>
/// Logger category class for PositionEventEndpoints
/// </summary>
public class PositionEventEndpointsLogger { }

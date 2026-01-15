using MongoDB.Driver;

namespace MinimalApiAot.Services;

/// <summary>
/// Service for managing position events with Portfolio synchronization and optimistic concurrency
/// </summary>
public class PositionEventService(
    MongoDbContext db,
    ILogger<PositionEventService> logger)
    : IPositionEventService
{
    private readonly IMongoCollection<PositionEvent> _positionEvents = db.PositionEvents;
    private readonly IMongoCollection<User> _users = db.Users;
    private readonly IMongoCollection<Stock> _stocks = db.Stocks;
    private readonly IMongoCollection<Portfolio> _portfolios = db.Portfolios;
    private const int MaxRetryAttempts = 3;
    private const decimal ValidationTolerance = 0.01m; // Allow small floating point differences

    #region Query Methods

    public async Task<PaginatedResponse<PositionEventResponseDto>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var filter = FilterDefinition<PositionEvent>.Empty;
        var totalCount = await _positionEvents.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _positionEvents.Find(filter)
            .SortByDescending(e => e.TradeAt)
            .Skip((int)pagination.Skip)
            .Limit((int)pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            (int)totalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<PositionEvent?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return await _positionEvents.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PositionEvent?> GetByOperationIdAsync(string operationId, CancellationToken cancellationToken = default)
    {
        return await _positionEvents.Find(e => e.OperationId == operationId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<PositionEventResponseDto>> GetByUserIdAsync(
        ObjectId userId,
        PositionEventQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.GetEffectiveStartDate();
        var endDate = query.GetEffectiveEndDate();
        var pagination = query.ToPaginationRequest();

        var filter = Builders<PositionEvent>.Filter.And(
            Builders<PositionEvent>.Filter.Eq(e => e.UserId, userId),
            Builders<PositionEvent>.Filter.Gte(e => e.TradeAt, startDate),
            Builders<PositionEvent>.Filter.Lte(e => e.TradeAt, endDate));

        if (query.Type.HasValue)
        {
            filter = Builders<PositionEvent>.Filter.And(
                filter,
                Builders<PositionEvent>.Filter.Eq(e => e.Type, query.Type.Value));
        }

        var totalCount = await _positionEvents.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _positionEvents.Find(filter)
            .SortByDescending(e => e.TradeAt)
            .Skip((int)pagination.Skip)
            .Limit((int)pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            (int)totalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<PaginatedResponse<PositionEventResponseDto>> GetByStockIdAsync(
        ObjectId stockId,
        PositionEventQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.GetEffectiveStartDate();
        var endDate = query.GetEffectiveEndDate();
        var pagination = query.ToPaginationRequest();

        var filter = Builders<PositionEvent>.Filter.And(
            Builders<PositionEvent>.Filter.Eq(e => e.StockId, stockId),
            Builders<PositionEvent>.Filter.Gte(e => e.TradeAt, startDate),
            Builders<PositionEvent>.Filter.Lte(e => e.TradeAt, endDate));

        if (query.Type.HasValue)
        {
            filter = Builders<PositionEvent>.Filter.And(
                filter,
                Builders<PositionEvent>.Filter.Eq(e => e.Type, query.Type.Value));
        }

        var totalCount = await _positionEvents.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _positionEvents.Find(filter)
            .SortByDescending(e => e.TradeAt)
            .Skip((int)pagination.Skip)
            .Limit((int)pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            (int)totalCount,
            pagination.Page,
            pagination.PageSize);
    }

    #endregion

    #region Create

    public async Task<PositionEventCreateResult> CreateAsync(
        CreatePositionEventRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate ObjectId formats
        if (!ObjectId.TryParse(request.UserId, out var userId))
        {
            return PositionEventCreateResult.Error("Invalid UserId format", PositionEventErrorType.ValidationFailed);
        }

        if (!ObjectId.TryParse(request.StockId, out var stockId))
        {
            return PositionEventCreateResult.Error("Invalid StockId format", PositionEventErrorType.ValidationFailed);
        }

        // Validate computed fields for BUY/SELL (REPLACE skips validation)
        var validationResult = ValidateComputedFields(request);
        if (!validationResult.IsValid)
        {
            return PositionEventCreateResult.Error(validationResult.ErrorMessage!, PositionEventErrorType.ValidationFailed);
        }

        // Check if user exists
        var userExists = await _users.Find(u => u.Id == userId).AnyAsync(cancellationToken);
        if (!userExists)
        {
            return PositionEventCreateResult.Error($"User with ID {request.UserId} not found", PositionEventErrorType.UserNotFound);
        }

        // Check if stock exists
        var stockExists = await _stocks.Find(s => s.Id == stockId).AnyAsync(cancellationToken);
        if (!stockExists)
        {
            return PositionEventCreateResult.Error($"Stock with ID {request.StockId} not found", PositionEventErrorType.StockNotFound);
        }

        // Create the position event entity
        var positionEvent = new PositionEvent
        {
            Id = ObjectId.GenerateNewId(),
            OperationId = request.OperationId,
            UserId = userId,
            StockId = stockId,
            Type = request.Type,
            TradeAt = request.TradeAt,
            CreatedAt = DateTime.UtcNow,
            QuantityBefore = request.QuantityBefore,
            QuantityAfter = request.QuantityAfter,
            QuantityDelta = request.QuantityDelta,
            Currency = request.Currency,
            TotalCostBefore = request.TotalCostBefore,
            TotalCostAfter = request.TotalCostAfter,
            UnitPrice = request.UnitPrice,
            Source = request.Source,
            AppVersion = request.AppVersion
        };

        // Retry loop for optimistic concurrency
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                var operationExists = await _positionEvents
                    .Find(e => e.OperationId == request.OperationId)
                    .AnyAsync(cancellationToken);

                if (operationExists)
                {
                    return PositionEventCreateResult.Error(
                        $"OperationId '{request.OperationId}' already exists",
                        PositionEventErrorType.DuplicateOperationId);
                }

                // Get the portfolio for this user
                var portfolio = await _portfolios
                    .Find(p => p.UserId == userId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (portfolio == null)
                {
                    return PositionEventCreateResult.Error($"Portfolio not found for user {request.UserId}", PositionEventErrorType.PortfolioNotFound);
                }

                // Update portfolio stock quantity
                UpdatePortfolioStock(portfolio, stockId, request.QuantityAfter);
                portfolio.LastUpdated = DateTime.UtcNow;
                var currentVersion = portfolio.Version ?? 0;
                portfolio.Version = currentVersion + 1;

                var updated = await TryUpdatePortfolioAsync(portfolio, currentVersion, cancellationToken);
                if (!updated)
                {
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        var delay = (int)Math.Pow(2, attempt) * 50;
                        logger.LogWarning(
                            "Concurrency conflict on attempt {Attempt}, retrying in {Delay}ms...",
                            attempt + 1, delay);
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }

                    logger.LogError("Concurrency conflict persisted after {MaxRetries} attempts", MaxRetryAttempts);
                    return PositionEventCreateResult.Error(
                        "Portfolio update conflict after maximum retries. Please try again.",
                        PositionEventErrorType.ConcurrencyConflict);
                }

                await _positionEvents.InsertOneAsync(positionEvent, cancellationToken: cancellationToken);

                logger.LogInformation(
                    "Created PositionEvent {OperationId} for User {UserId}, Stock {StockId}, Type {Type}, QuantityDelta {QuantityDelta}",
                    positionEvent.OperationId, userId, stockId, positionEvent.Type, positionEvent.QuantityDelta);

                return PositionEventCreateResult.Ok(positionEvent);
            }
            catch (MongoWriteException ex) when (IsDuplicateKeyException(ex))
            {
                logger.LogWarning("Duplicate operationId detected: {OperationId}", request.OperationId);
                return PositionEventCreateResult.Error(
                    $"OperationId '{request.OperationId}' already exists",
                    PositionEventErrorType.DuplicateOperationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating position event with operationId {OperationId}", request.OperationId);
                return PositionEventCreateResult.Error(
                    $"Database error: {ex.Message}",
                    PositionEventErrorType.DatabaseError);
            }
        }

        return PositionEventCreateResult.Error(
            "Unexpected error after retry loop",
            PositionEventErrorType.DatabaseError);
    }

    #endregion

    #region Update

    public async Task<PositionEventUpdateResult> UpdateAsync(
        ObjectId id,
        UpdatePositionEventRequest request,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                var positionEvent = await _positionEvents
                    .Find(e => e.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (positionEvent == null)
                {
                    return PositionEventUpdateResult.Error(
                        $"PositionEvent with ID {id} not found",
                        PositionEventErrorType.NotFound);
                }

                // Store old quantityAfter for portfolio rollback calculation
                var oldQuantityAfter = positionEvent.QuantityAfter;

                // Apply updates
                if (request.QuantityBefore.HasValue)
                    positionEvent.QuantityBefore = request.QuantityBefore.Value;
                if (request.QuantityAfter.HasValue)
                    positionEvent.QuantityAfter = request.QuantityAfter.Value;
                if (request.QuantityDelta.HasValue)
                    positionEvent.QuantityDelta = request.QuantityDelta.Value;
                if (request.TotalCostBefore.HasValue)
                    positionEvent.TotalCostBefore = request.TotalCostBefore.Value;
                if (request.TotalCostAfter.HasValue)
                    positionEvent.TotalCostAfter = request.TotalCostAfter.Value;
                if (request.UnitPrice.HasValue)
                    positionEvent.UnitPrice = request.UnitPrice.Value;

                // If quantityAfter changed, sync portfolio
                if (request.QuantityAfter.HasValue && request.QuantityAfter.Value != oldQuantityAfter)
                {
                    var portfolio = await _portfolios
                        .Find(p => p.UserId == positionEvent.UserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (portfolio != null)
                    {
                        UpdatePortfolioStock(portfolio, positionEvent.StockId, request.QuantityAfter.Value);
                        portfolio.LastUpdated = DateTime.UtcNow;
                        var currentVersion = portfolio.Version ?? 0;
                        portfolio.Version = currentVersion + 1;

                        var updated = await TryUpdatePortfolioAsync(portfolio, currentVersion, cancellationToken);
                        if (!updated)
                        {
                            if (attempt < MaxRetryAttempts - 1)
                            {
                                var delay = (int)Math.Pow(2, attempt) * 50;
                                logger.LogWarning("Concurrency conflict on update attempt {Attempt}, retrying...", attempt + 1);
                                await Task.Delay(delay, cancellationToken);
                                continue;
                            }

                            return PositionEventUpdateResult.Error(
                                "Portfolio update conflict after maximum retries",
                                PositionEventErrorType.ConcurrencyConflict);
                        }
                    }
                }

                await _positionEvents.ReplaceOneAsync(
                    e => e.Id == id,
                    positionEvent,
                    cancellationToken: cancellationToken);

                logger.LogInformation("Updated PositionEvent {Id}", id);
                return PositionEventUpdateResult.Ok(positionEvent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating position event {Id}", id);
                return PositionEventUpdateResult.Error(
                    $"Database error: {ex.Message}",
                    PositionEventErrorType.DatabaseError);
            }
        }

        return PositionEventUpdateResult.Error(
            "Unexpected error after retry loop",
            PositionEventErrorType.DatabaseError);
    }

    #endregion

    #region Delete

    public async Task<PositionEventDeleteResult> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                var positionEvent = await _positionEvents
                    .Find(e => e.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (positionEvent == null)
                {
                    return PositionEventDeleteResult.Error(
                        $"PositionEvent with ID {id} not found",
                        PositionEventErrorType.NotFound);
                }

                // Rollback portfolio quantity
                var portfolio = await _portfolios
                    .Find(p => p.UserId == positionEvent.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (portfolio != null)
                {
                    // Rollback: set quantity back to quantityBefore (use 0 if null for old records)
                    var rolledBackQuantity = positionEvent.QuantityBefore ?? 0m;
                    UpdatePortfolioStock(portfolio, positionEvent.StockId, rolledBackQuantity);
                    portfolio.LastUpdated = DateTime.UtcNow;
                    var currentVersion = portfolio.Version ?? 0;
                    portfolio.Version = currentVersion + 1;

                    var updated = await TryUpdatePortfolioAsync(portfolio, currentVersion, cancellationToken);
                    if (!updated)
                    {
                        if (attempt < MaxRetryAttempts - 1)
                        {
                            var delay = (int)Math.Pow(2, attempt) * 50;
                            logger.LogWarning("Concurrency conflict on delete attempt {Attempt}, retrying...", attempt + 1);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        return PositionEventDeleteResult.Error(
                            "Portfolio update conflict after maximum retries",
                            PositionEventErrorType.ConcurrencyConflict);
                    }
                }

                await _positionEvents.DeleteOneAsync(e => e.Id == id, cancellationToken);

                logger.LogInformation(
                    "Deleted PositionEvent {Id} and rolled back Portfolio quantity to {Quantity}",
                    id, positionEvent.QuantityBefore);

                return PositionEventDeleteResult.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting position event {Id}", id);
                return PositionEventDeleteResult.Error(
                    $"Database error: {ex.Message}",
                    PositionEventErrorType.DatabaseError);
            }
        }

        return PositionEventDeleteResult.Error(
            "Unexpected error after retry loop",
            PositionEventErrorType.DatabaseError);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Update portfolio stock quantity, adding new stock if needed, removing if quantity is zero
    /// </summary>
    private static void UpdatePortfolioStock(Portfolio portfolio, ObjectId stockId, decimal quantity)
    {
        var stockEntry = portfolio.Stocks.FirstOrDefault(s => s.StockId == stockId);

        if (quantity == 0)
        {
            // Remove stock if quantity is zero
            if (stockEntry != null)
            {
                portfolio.Stocks.Remove(stockEntry);
            }
        }
        else if (stockEntry == null)
        {
            // Add new stock entry
            portfolio.Stocks.Add(new PortfolioStock
            {
                StockId = stockId,
                Quantity = quantity
            });
        }
        else
        {
            // Update existing stock quantity
            stockEntry.Quantity = quantity;
        }
    }

    private async Task<bool> TryUpdatePortfolioAsync(
        Portfolio portfolio,
        long currentVersion,
        CancellationToken cancellationToken)
    {
        var versionFilter = currentVersion == 0
            ? Builders<Portfolio>.Filter.Or(
                Builders<Portfolio>.Filter.Eq(p => p.Version, 0),
                Builders<Portfolio>.Filter.Eq(p => p.Version, (long?)null))
            : Builders<Portfolio>.Filter.Eq(p => p.Version, currentVersion);

        var filter = Builders<Portfolio>.Filter.And(
            Builders<Portfolio>.Filter.Eq(p => p.Id, portfolio.Id),
            versionFilter);

        var update = Builders<Portfolio>.Update
            .Set(p => p.Stocks, portfolio.Stocks)
            .Set(p => p.LastUpdated, portfolio.LastUpdated)
            .Set(p => p.Version, portfolio.Version);

        var result = await _portfolios.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// Validate computed fields for BUY/SELL operations
    /// REPLACE operations skip validation as they completely replace the position
    /// </summary>
    private static ValidationResult ValidateComputedFields(CreatePositionEventRequest request)
    {
        // REPLACE skips validation - directly accepts iOS values
        if (request.Type == PositionEventType.REPLACE)
        {
            return ValidationResult.Valid();
        }

        // Validate quantityAfter = quantityBefore + quantityDelta
        var expectedQuantityAfter = request.QuantityBefore + request.QuantityDelta;
        if (Math.Abs(request.QuantityAfter - expectedQuantityAfter) > ValidationTolerance)
        {
            return ValidationResult.Invalid(
                $"Invalid quantityAfter: expected {expectedQuantityAfter} (quantityBefore {request.QuantityBefore} + quantityDelta {request.QuantityDelta}), got {request.QuantityAfter}");
        }

        // Validate totalCostAfter based on operation type
        if (request.Type == PositionEventType.BUY)
        {
            // BUY: totalCostAfter = totalCostBefore + (quantityDelta * unitPrice)
            var expectedTotalCostAfter = request.TotalCostBefore + (request.QuantityDelta * request.UnitPrice);
            if (Math.Abs(request.TotalCostAfter - expectedTotalCostAfter) > ValidationTolerance)
            {
                return ValidationResult.Invalid(
                    $"Invalid totalCostAfter for BUY: expected {expectedTotalCostAfter:F2}, got {request.TotalCostAfter:F2}");
            }
        }
        else if (request.Type == PositionEventType.SELL)
        {
            // SELL: totalCostAfter = totalCostBefore - (|quantityDelta| * averageCost)
            // where averageCost = totalCostBefore / quantityBefore
            if (request.QuantityBefore > 0)
            {
                var averageCost = request.TotalCostBefore / request.QuantityBefore;
                var expectedTotalCostAfter = request.TotalCostBefore - (Math.Abs(request.QuantityDelta) * averageCost);
                if (Math.Abs(request.TotalCostAfter - expectedTotalCostAfter) > ValidationTolerance)
                {
                    return ValidationResult.Invalid(
                        $"Invalid totalCostAfter for SELL: expected {expectedTotalCostAfter:F2} (using average cost {averageCost:F2}), got {request.TotalCostAfter:F2}");
                }
            }
        }

        // Validate quantityDelta sign matches operation type
        if (request.Type == PositionEventType.BUY && request.QuantityDelta < 0)
        {
            return ValidationResult.Invalid("BUY operation should have positive quantityDelta");
        }

        if (request.Type == PositionEventType.SELL && request.QuantityDelta > 0)
        {
            return ValidationResult.Invalid("SELL operation should have negative quantityDelta");
        }

        // Validate SELL doesn't result in negative quantity
        if (request.Type == PositionEventType.SELL && request.QuantityAfter < 0)
        {
            return ValidationResult.Invalid("SELL operation cannot result in negative quantity");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Check if exception is caused by duplicate key (operationId unique constraint)
    /// </summary>
    private static bool IsDuplicateKeyException(MongoWriteException ex)
    {
        return ex.WriteError?.Category == ServerErrorCategory.DuplicateKey;
    }

    #endregion

    #region Validation Result

    private record ValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    #endregion
}

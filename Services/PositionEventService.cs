namespace MinimalApiAot.Services;

/// <summary>
/// Service for managing position events with Portfolio synchronization and optimistic concurrency
/// </summary>
public class PositionEventService(
    ApplicationDbContext context,
    ILogger<PositionEventService> logger)
    : IPositionEventService
{
    private const int MaxRetryAttempts = 3;
    private const decimal ValidationTolerance = 0.01m; // Allow small floating point differences

    #region Query Methods

    public async Task<PaginatedResponse<PositionEventResponseDto>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = context.PositionEvents.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.TradeAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            totalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<PositionEvent?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return await context.PositionEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<PositionEvent?> GetByOperationIdAsync(string operationId, CancellationToken cancellationToken = default)
    {
        return await context.PositionEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.OperationId == operationId, cancellationToken);
    }

    public async Task<PaginatedResponse<PositionEventResponseDto>> GetByUserIdAsync(
        ObjectId userId,
        PositionEventQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var startDate = query.GetEffectiveStartDate();
        var endDate = query.GetEffectiveEndDate();
        var pagination = query.ToPaginationRequest();

        var baseQuery = context.PositionEvents
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Where(e => e.TradeAt >= startDate && e.TradeAt <= endDate);

        // Apply type filter if specified
        if (query.Type.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.Type == query.Type.Value);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(e => e.TradeAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            totalCount,
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

        var baseQuery = context.PositionEvents
            .AsNoTracking()
            .Where(e => e.StockId == stockId)
            .Where(e => e.TradeAt >= startDate && e.TradeAt <= endDate);

        // Apply type filter if specified
        if (query.Type.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.Type == query.Type.Value);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(e => e.TradeAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<PositionEventResponseDto>.Create(
            items.Select(PositionEventResponseDto.FromEntity),
            totalCount,
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
        var userExists = await context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            return PositionEventCreateResult.Error($"User with ID {request.UserId} not found", PositionEventErrorType.UserNotFound);
        }

        // Check if stock exists
        var stockExists = await context.Stocks.AnyAsync(s => s.Id == stockId, cancellationToken);
        if (!stockExists)
        {
            return PositionEventCreateResult.Error($"Stock with ID {request.StockId} not found", PositionEventErrorType.StockNotFound);
        }

        // Create the position event entity
        var positionEvent = new PositionEvent
        {
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
                // Get the portfolio for this user
                var portfolio = await context.Portfolios
                    .Include(p => p.Stocks)
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                if (portfolio == null)
                {
                    return PositionEventCreateResult.Error($"Portfolio not found for user {request.UserId}", PositionEventErrorType.PortfolioNotFound);
                }

                // Update portfolio stock quantity
                UpdatePortfolioStock(portfolio, stockId, request.QuantityAfter);
                portfolio.LastUpdated = DateTime.UtcNow;
                portfolio.Version = (portfolio.Version ?? 0) + 1;

                // Add the position event
                await context.PositionEvents.AddAsync(positionEvent, cancellationToken);

                // Save both changes
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Created PositionEvent {OperationId} for User {UserId}, Stock {StockId}, Type {Type}, QuantityDelta {QuantityDelta}",
                    positionEvent.OperationId, userId, stockId, positionEvent.Type, positionEvent.QuantityDelta);

                return PositionEventCreateResult.Ok(positionEvent);
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                logger.LogWarning("Duplicate operationId detected: {OperationId}", request.OperationId);
                return PositionEventCreateResult.Error(
                    $"OperationId '{request.OperationId}' already exists",
                    PositionEventErrorType.DuplicateOperationId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt < MaxRetryAttempts - 1)
                {
                    var delay = (int)Math.Pow(2, attempt) * 50; // 50ms, 100ms, 200ms
                    logger.LogWarning(
                        "Concurrency conflict on attempt {Attempt}, retrying in {Delay}ms...",
                        attempt + 1, delay);
                    await Task.Delay(delay, cancellationToken);

                    // Clear the change tracker to get fresh data
                    context.ChangeTracker.Clear();
                }
                else
                {
                    logger.LogError("Concurrency conflict persisted after {MaxRetries} attempts", MaxRetryAttempts);
                    return PositionEventCreateResult.Error(
                        "Portfolio update conflict after maximum retries. Please try again.",
                        PositionEventErrorType.ConcurrencyConflict);
                }
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
                var positionEvent = await context.PositionEvents
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

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
                    var portfolio = await context.Portfolios
                        .Include(p => p.Stocks)
                        .FirstOrDefaultAsync(p => p.UserId == positionEvent.UserId, cancellationToken);

                    if (portfolio != null)
                    {
                        UpdatePortfolioStock(portfolio, positionEvent.StockId, request.QuantityAfter.Value);
                        portfolio.LastUpdated = DateTime.UtcNow;
                        portfolio.Version = (portfolio.Version ?? 0) + 1;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Updated PositionEvent {Id}", id);
                return PositionEventUpdateResult.Ok(positionEvent);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt < MaxRetryAttempts - 1)
                {
                    var delay = (int)Math.Pow(2, attempt) * 50;
                    logger.LogWarning("Concurrency conflict on update attempt {Attempt}, retrying...", attempt + 1);
                    await Task.Delay(delay, cancellationToken);
                    context.ChangeTracker.Clear();
                }
                else
                {
                    return PositionEventUpdateResult.Error(
                        "Portfolio update conflict after maximum retries",
                        PositionEventErrorType.ConcurrencyConflict);
                }
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
                var positionEvent = await context.PositionEvents
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

                if (positionEvent == null)
                {
                    return PositionEventDeleteResult.Error(
                        $"PositionEvent with ID {id} not found",
                        PositionEventErrorType.NotFound);
                }

                // Rollback portfolio quantity
                var portfolio = await context.Portfolios
                    .Include(p => p.Stocks)
                    .FirstOrDefaultAsync(p => p.UserId == positionEvent.UserId, cancellationToken);

                if (portfolio != null)
                {
                    // Rollback: set quantity back to quantityBefore (use 0 if null for old records)
                    var rolledBackQuantity = positionEvent.QuantityBefore ?? 0m;
                    UpdatePortfolioStock(portfolio, positionEvent.StockId, rolledBackQuantity);
                    portfolio.LastUpdated = DateTime.UtcNow;
                    portfolio.Version = (portfolio.Version ?? 0) + 1;
                }

                context.PositionEvents.Remove(positionEvent);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Deleted PositionEvent {Id} and rolled back Portfolio quantity to {Quantity}",
                    id, positionEvent.QuantityBefore);

                return PositionEventDeleteResult.Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt < MaxRetryAttempts - 1)
                {
                    var delay = (int)Math.Pow(2, attempt) * 50;
                    logger.LogWarning("Concurrency conflict on delete attempt {Attempt}, retrying...", attempt + 1);
                    await Task.Delay(delay, cancellationToken);
                    context.ChangeTracker.Clear();
                }
                else
                {
                    return PositionEventDeleteResult.Error(
                        "Portfolio update conflict after maximum retries",
                        PositionEventErrorType.ConcurrencyConflict);
                }
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
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // MongoDB duplicate key error contains "E11000" or "duplicate key"
        return ex.InnerException?.Message?.Contains("E11000") == true ||
               ex.InnerException?.Message?.Contains("duplicate key") == true ||
               ex.Message.Contains("E11000") ||
               ex.Message.Contains("duplicate key");
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

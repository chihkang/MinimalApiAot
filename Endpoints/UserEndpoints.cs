namespace MinimalApiAot.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        // 獲取所有使用者
        group.MapGet("/", async (IUserService userService, ILogger<IUserService> logger) =>
            {
                try
                {
                    var users = await userService.GetAllUsersAsync();
                    return users.Any()
                        ? Results.Ok(users)
                        : Results.NoContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "查詢使用者資料時發生錯誤");
                    return Results.Problem("查詢使用者資料時發生錯誤");
                }
            })
            .WithName("GetAllUsers")
            .WithOpenApi();

        // 依 ID 獲取使用者
        group.MapGet("/{id}", async (string id, IUserService userService, ILogger<IUserService> logger) =>
            {
                try
                {
                    var user = await userService.GetUserByIdAsync(id);
                    return user != null ? Results.Ok(user) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "查詢使用者資料時發生錯誤");
                    return Results.Problem("查詢使用者資料時發生錯誤");
                }
            })
            .WithName("GetUserById")
            .WithOpenApi();

        // 依電子郵件查詢使用者
        group.MapGet("/by-email/{email}",
                async (string email, IUserService userService, ILogger<IUserService> logger) =>
                {
                    try
                    {
                        var user = await userService.GetUserByEmailAsync(email);
                        return user != null ? Results.Ok(user) : Results.NotFound();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "查詢使用者資料時發生錯誤");
                        return Results.Problem("查詢使用者資料時發生錯誤");
                    }
                })
            .WithName("GetUserByEmail")
            .WithOpenApi();

        // 建立新使用者
        group.MapPost("/", async (User user, IUserService userService, ILogger<IUserService> logger) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Username))
                    {
                        return Results.BadRequest("使用者名稱和電子郵件為必填欄位");
                    }

                    var createdUser = await userService.CreateUserAsync(user);
                    return Results.Created($"/api/users/{createdUser.Id}", createdUser);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "建立使用者時發生錯誤");
                    return Results.Problem("建立使用者時發生錯誤");
                }
            })
            .WithName("CreateUser")
            .WithOpenApi();

        // 更新使用者
        group.MapPut("/{id}", async (string id, User user, IUserService userService, ILogger<IUserService> logger) =>
            {
                try
                {
                    var success = await userService.UpdateUserAsync(id, user);
                    return success ? Results.Ok() : Results.NotFound();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "更新使用者資料時發生錯誤");
                    return Results.Problem("更新使用者資料時發生錯誤");
                }
            })
            .WithName("UpdateUser")
            .WithOpenApi();

        // 刪除使用者
        group.MapDelete("/{id}", async (string id, IUserService userService, ILogger<IUserService> logger) =>
            {
                try
                {
                    var success = await userService.DeleteUserAsync(id);
                    return success ? Results.Ok() : Results.NotFound();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "刪除使用者時發生錯誤");
                    return Results.Problem("刪除使用者時發生錯誤");
                }
            })
            .WithName("DeleteUser")
            .WithOpenApi();
    }
}
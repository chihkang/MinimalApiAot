namespace MinimalApiAot.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");
        // Get all users
        group.MapGet("/", async (IUserService userService) =>
        {
            var users = await userService.GetAllUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .Produces<List<User>>();

        // Get user by ID
        group.MapGet("/{id}", async (string id, IUserService userService) =>
        {
            var user = await userService.GetUserByIdAsync(id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .WithName("GetUserById")
        .Produces<User>()
        .Produces(404);

        // Create user
        group.MapPost("/", async ([FromBody]User user, IUserService userService) =>
        {
            var createdUser = await userService.CreateUserAsync(user);
            return Results.Created($"/api/users/{createdUser.Id}", createdUser);
        })
        .WithName("CreateUser")
        .Produces<User>(201);

        // Update user
        group.MapPut("/{id}", async (string id, [FromBody]User user, IUserService userService) =>
        {
            user.Id = id;
            var result = await userService.UpdateUserAsync(id, user);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUser")
        .Produces(204)
        .Produces(404);

        // Delete user
        group.MapDelete("/{id}", async (string id, IUserService userService) =>
        {
            var result = await userService.DeleteUserAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUser")
        .Produces(204)
        .Produces(404);

        // Get user by email
        group.MapGet("/email/{email}", async (string email, IUserService userService) =>
        {
            var user = await userService.GetUserByEmailAsync(email);
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .WithName("GetUserByEmail")
        .Produces<User>()
        .Produces(404);
    }
}
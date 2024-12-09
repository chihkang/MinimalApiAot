namespace MinimalApiAot.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        app.MapGet("/api/items", async ([FromServices]IMongoRepository repo) =>
                await repo.GetAllAsync<Item>())
            .WithName("GetItems");

        app.MapGet("/api/items/{id}", async (string id, [FromServices]IMongoRepository repo) =>
            {
                var item = await repo.GetByIdAsync<Item>(id);
                return item is null ? Results.NotFound() : Results.Ok(item);
            })
            .WithName("GetItem");

        app.MapPost("/api/items", async ([FromBody]Item item, [FromServices]IMongoRepository repo) =>
            {
                await repo.CreateAsync(item);
                return Results.Created($"/api/items/{item.Id}", item);
            })
            .WithName("CreateItem");

        app.MapPut("/api/items/{id}", async (string id, [FromBody]Item item, [FromServices]IMongoRepository repo) =>
            {
                await repo.UpdateAsync(id, item);
                return Results.NoContent();
            })
            .WithName("UpdateItem");

        app.MapDelete("/api/items/{id}", async (string id, [FromServices]IMongoRepository repo) =>
            {
                await repo.DeleteAsync<Item>(id);
                return Results.NoContent();
            })
            .WithName("DeleteItem");
    }
}
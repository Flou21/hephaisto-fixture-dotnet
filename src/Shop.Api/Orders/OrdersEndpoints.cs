namespace Shop.Api.Orders;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrders(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/orders");

        orders.MapGet("/", (OrderStore store) => Results.Ok(store.All()));

        orders.MapGet("/{id:int}", (int id, OrderStore store) =>
            store.TryGet(id, out var order) ? Results.Ok(order) : Results.NotFound());

        orders.MapPost("/", (NewOrder body, OrderStore store) =>
        {
            if (string.IsNullOrWhiteSpace(body.Sku) || body.Quantity <= 0)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["order"] = ["sku is required and quantity must be positive"],
                });

            var created = store.Add(body);

            return Results.Created($"/orders/{created.Id}", created);
        });

        return app;
    }
}

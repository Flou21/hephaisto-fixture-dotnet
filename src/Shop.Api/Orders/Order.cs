namespace Shop.Api.Orders;

public sealed record Order(int Id, string Sku, int Quantity, DateTimeOffset CreatedAt);

public sealed record NewOrder(string Sku, int Quantity);

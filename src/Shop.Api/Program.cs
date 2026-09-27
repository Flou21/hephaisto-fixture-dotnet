using Microsoft.Extensions.Options;
using Shop.Api;
using Shop.Api.Orders;
using Shop.Api.Startup;

var builder = WebApplication.CreateBuilder(args);

Notices.Print(builder.Configuration, Console.Out);

builder.Services.AddOptions<ShopOptions>().Bind(builder.Configuration.GetSection(ShopOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();

var shop = app.Services.GetRequiredService<IOptions<ShopOptions>>().Value;
var primary = Endpoints.Primary(shop);

app.Logger.LogInformation("{Shop} forwarding order events to {Endpoint}", shop.Name, primary);

app.MapGet("/healthz", () => Results.Ok(new { status = "ok", shop = shop.Name, endpoint = primary }));
app.MapOrders();

app.Run();

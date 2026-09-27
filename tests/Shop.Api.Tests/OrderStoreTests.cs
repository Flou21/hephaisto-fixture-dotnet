using Shop.Api.Orders;

namespace Shop.Api.Tests;

public class OrderStoreTests
{
    [Fact]
    public void Added_orders_get_increasing_ids()
    {
        var store = new OrderStore(TimeProvider.System);

        var first = store.Add(new NewOrder("sku-1", 1));
        var second = store.Add(new NewOrder("sku-2", 2));

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal([first, second], store.All());
    }

    [Fact]
    public void Missing_order_is_not_found()
    {
        var store = new OrderStore(TimeProvider.System);

        Assert.False(store.TryGet(42, out var order));
        Assert.Null(order);
    }
}

using System.Collections.Concurrent;

namespace Shop.Api.Orders;

/// <summary>
/// In-memory order store. One per process; nothing survives a restart, by design.
/// </summary>
public sealed class OrderStore(TimeProvider clock)
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _nextId;

    public Order Add(NewOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var id = Interlocked.Increment(ref _nextId);
        var created = new Order(id, order.Sku, order.Quantity, clock.GetUtcNow());

        _orders[id] = created;

        return created;
    }

    public bool TryGet(int id, out Order? order) => _orders.TryGetValue(id, out order);

    public IReadOnlyList<Order> All() => [.. _orders.Values.OrderBy(o => o.Id)];
}

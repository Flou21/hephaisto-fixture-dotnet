namespace Shop.Api;

/// <summary>
/// Everything under the <c>Shop</c> configuration section.
/// </summary>
public sealed class ShopOptions
{
    public const string SectionName = "Shop";

    /// <summary>
    /// Upstream endpoints order events are forwarded to, in priority order.
    /// </summary>
    /// <remarks>
    /// Optional. When <c>Shop:Endpoints</c> is absent the configuration binder leaves this
    /// <c>null</c> rather than empty - there is nothing to bind, so nothing is created. Callers
    /// have to treat null and empty the same way.
    /// </remarks>
    public List<string>? Endpoints { get; set; }

    /// <summary>Shown in <c>/healthz</c> so a response says which shop answered.</summary>
    public string Name { get; set; } = "shop";
}

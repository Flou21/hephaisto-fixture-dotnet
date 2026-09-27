namespace Shop.Api.Startup;

/// <summary>
/// Resolves, once at startup, where order events are forwarded.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// What <see cref="Primary"/> returns when nothing is configured: events stay in-process.
    /// </summary>
    public const string Local = "local";

    public static string Primary(ShopOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Endpoints.Count == 0)
            return Local;

        return options.Endpoints[0];
    }
}

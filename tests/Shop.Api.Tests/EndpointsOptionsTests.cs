using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shop.Api.Startup;

namespace Shop.Api.Tests;

public class EndpointsOptionsTests
{
    /// <summary>
    /// A pod started with no <c>Shop:Endpoints</c> at all must come up, forwarding locally.
    /// </summary>
    /// <remarks>
    /// Bound through the same call <c>Program.cs</c> makes, from a configuration that has no
    /// <c>Shop</c> section - which is what the binder sees in a container whose manifest sets
    /// no <c>Shop__Endpoints__N</c> variable. The list is then <c>null</c>, not empty.
    /// </remarks>
    [Fact]
    public void Empty_endpoints_do_not_throw()
    {
        var options = Bind(new Dictionary<string, string?>());

        var primary = Endpoints.Primary(options);

        Assert.Equal(Endpoints.Local, primary);
    }

    [Fact]
    public void Configured_endpoints_pick_the_first()
    {
        var options = Bind(new Dictionary<string, string?>
        {
            ["Shop:Endpoints:0"] = "http://events-a:8080",
            ["Shop:Endpoints:1"] = "http://events-b:8080",
        });

        Assert.Equal("http://events-a:8080", Endpoints.Primary(options));
    }

    [Fact]
    public void Shop_name_binds_without_endpoints()
    {
        var options = Bind(new Dictionary<string, string?> { ["Shop:Name"] = "eu-shop" });

        Assert.Equal("eu-shop", options.Name);
    }

    private static ShopOptions Bind(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        var services = new ServiceCollection();
        services.AddOptions<ShopOptions>().Bind(configuration.GetSection(ShopOptions.SectionName));

        return services.BuildServiceProvider().GetRequiredService<IOptions<ShopOptions>>().Value;
    }
}

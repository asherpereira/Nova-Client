using Nova.Client.Core.Configuration;
using Xunit;

namespace Nova.Client.Core.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void WebSocketUriUsesConfiguredHost()
    {
        var options = new NovaClientOptions(
            ClientEnvironment.Development,
            new Uri("https://example.test/api/"));

        Assert.Equal("https://example.test/api/ws", options.WebSocketUri.ToString());
    }
}

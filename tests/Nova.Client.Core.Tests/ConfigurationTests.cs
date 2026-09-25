using Nova.Client.Core.Configuration;
namespace Nova.Client.Core.Tests;
public sealed class ConfigurationTests { [Fact] public void WebSocketUriUsesConfiguredHost(){var o=new NovaClientOptions(ClientEnvironment.Development,new Uri("https://example.test/api/"));Assert.Equal("https://example.test/api/ws",o.WebSocketUri.ToString());} }

using Nova.Client.Protocol;
using Xunit;

namespace Nova.Client.Protocol.Tests;

public sealed class ContractTests
{
    [Fact]
    public void CoreRoutesRemainVersioned()
    {
        Assert.StartsWith("/api/v1/", ApiRoutes.Login);
        Assert.Equal("/ws", ApiRoutes.WebSocket);
    }
}

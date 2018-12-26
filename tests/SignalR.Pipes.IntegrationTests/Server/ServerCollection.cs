using Xunit;

namespace SignalR.Pipes.IntegrationTests.Server
{
    [CollectionDefinition(Name)]
    public class ServerCollection : ICollectionFixture<ServerFixture>
    {
        public const string Name = "Server";
    }
}

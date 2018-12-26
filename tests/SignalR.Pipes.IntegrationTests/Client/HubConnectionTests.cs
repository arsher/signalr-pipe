using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Pipes.Client;
using SignalR.Pipes.IntegrationTests.Server;
using Xunit;

namespace SignalR.Pipes.IntegrationTests.Client
{
    [Collection(ServerCollection.Name)]
    public class HubConnectionTests
    {
        [Fact]
        public async Task EchoWorks()
        {
            var connection = new NamedPipeHubConnectionBuilder()
                .WithUri("signalr.pipe://testhost/testpath/net")
                .Build();

            try
            {
                await connection.StartAsync();

                Assert.Equal("test", await connection.InvokeAsync<string>("Echo", "test"));
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalR.Pipes.Client;
using SignalR.Pipes.Ipc;
using SignalR.Pipes.Ipc.Client;
using SignalR.Pipes.Ipc.Configuration;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SignalR.Ipc.IntegrationTests
{
    public class UnitTest1
    {
        public interface IClient
        {
            Task<int> Add(int onw, int two);
        }

        public interface IServer
        {
            Task Sun(string hello);
        }

        private class TestHub : IpcHub<IClient>, IServer
        {
            private readonly IIpcHubContext<TestHub, IClient> hubContext;

            public TestHub(IIpcHubContext<TestHub, IClient> hubContext)
            {
                this.hubContext = hubContext;
            }

            public override Task OnConnectedAsync()
            {

                var invokeClient = hubContext.Clients.Client(Context.ConnectionId);

                _ = Task.Run(async () =>
                {
                    var result = await invokeClient.Add(1, 2);
                    Console.WriteLine(result);
                });
                return base.OnConnectedAsync();
            }

            public Task Sun(string hello)
            {
                return Task.CompletedTask;
            }
        }

        private class Client : IClient
        {
            public Task<int> Add(int onw, int two)
            {
                return Task.FromResult(onw + two);
            }
        }

        [Fact]
        public async Task Test1()
        {
            var host = new HostBuilder()
               .UseHostUri(new Uri("signalr.pipe://testhost/"))
               .UseSignalR(b =>
               {
                   b.MapHub<TestHub>("/testpath/net");
               })
               .ConfigureServices(collection =>
               {
                   collection.AddSignalR()
                    .AddIpc();
               })
               .Build();

            await host.StartAsync();

            var client = new IpcHubConnectionBuilder<Client>(new Client())
                .WithUri("signalr.pipe://testhost/testpath/net")
                .Build();

            await client.StartAsync();

            var server = client.GetServer<IServer>();
            await server.Sun();

            await Task.Delay(1000000);
        }
    }
}

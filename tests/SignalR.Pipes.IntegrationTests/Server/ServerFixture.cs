using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalR.Pipes.Configuration;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public class ServerFixture : IDisposable
    {
        private readonly IHost host;

        public ServerFixture()
        {
            host = new HostBuilder()
                .UseHostUri(new Uri("signalr.pipe://testhost/"))
                .UseSignalR(b =>
                {
                    b.MapHub<TestHub>("/testpath/net");
                })
                .ConfigureServices(collection =>
                {
                    collection.AddSignalR();
                })
                .Build();

            host.Start();
        }

        public void Dispose()
        {
            host.StopAsync().Wait();
        }
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public class ServerFixture : IDisposable
    {
        private readonly IHost host;
        private IDisposable disposable;


        public ServerFixture()
        {
            host = new HostBuilder()
                .UseDisposableLifetime(d => disposable = d)
                .ConfigureServices(collection =>
                {
                    collection.AddSignalR()
                        .AddHub<TestHub>("signalr.pipe://testhost/testpath/net");
                })
                .Build();

            host.Start();
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}

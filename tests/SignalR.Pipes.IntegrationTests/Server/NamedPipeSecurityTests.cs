using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Xunit;

namespace SignalR.Pipes.IntegrationTests.Server
{
    [Collection(ServerCollection.Name)]
    public class NamedPipeSecurityTests
    {
        [Fact]
        public void CantHostOnSameName()
        {
            Assert.Throws<IOException>(() =>
            {
                var host = new HostBuilder()
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
            });
        }
    }
}

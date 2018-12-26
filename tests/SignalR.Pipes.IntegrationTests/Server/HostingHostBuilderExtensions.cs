using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public static class HostingHostBuilderExtensions
    {
        public static IHostBuilder UseDisposableLifetime(this IHostBuilder hostBuilder,
            Action<IDisposable> disposableSetter)
        {
            return hostBuilder.ConfigureServices((context, collection) =>
            {
                collection.AddOptions<DisposableLifetimeOptions>()
                    .Configure(o => o.DisposableSetter = disposableSetter);
                collection.AddSingleton<IHostLifetime, DisposableLifetime>();
            });
        }

    }
}

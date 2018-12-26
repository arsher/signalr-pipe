using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SignalR.Pipes.Configuration;
using SignalR.Pipes.Connections;
using SignalR.Pipes.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRDependencyInjectionExtensions
    {
        public static ISignalRServerBuilder AddHubOptions<THub>(this ISignalRServerBuilder signalrBuilder, Action<HubOptions<THub>> configure) where THub : Hub
        {
            signalrBuilder.Services.AddSingleton<IConfigureOptions<HubOptions<THub>>, HubOptionsSetup<THub>>();
            signalrBuilder.Services.Configure(configure);
            return signalrBuilder;
        }

        public static ISignalRServerBuilder AddHub<THub>(this ISignalRServerBuilder signalrBuilder, string uriString) where THub : Hub
        {
            var uri = new Uri(uriString);
            return signalrBuilder.AddHub<THub>(uri);
        }

        public static ISignalRServerBuilder AddHub<THub>(this ISignalRServerBuilder signalrBuilder, Uri uri) where THub : Hub
        {
            signalrBuilder.Services.Configure<HubRouteOptions>(c => c.AddHub(uri, npcm => npcm.UseHub<THub>()));
            return signalrBuilder;
        }

        public static ISignalRServerBuilder AddSignalR(this IServiceCollection services)
        {
            services.AddSingleton<NamedPipeServerManager>();
            services.AddSingleton<NamedPipeConnectionDispatcher>();
            services.AddHostedService<SignalRConfigurationService>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HubOptions>, HubOptionsSetup>());
            return services.AddSignalRCore();
        }
    }
}

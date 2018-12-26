using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using SignalR.Pipes.Connections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Connections;
using SignalR.Pipes.Common;

namespace SignalR.Pipes.Services
{
    public class SignalRConfigurationService : IHostedService
    {
        private readonly HubRouteOptions hubRouteOptions;
        private readonly IServiceProvider serviceProvider;

        public SignalRConfigurationService(IServiceProvider serviceProvider, IOptions<HubRouteOptions> hubRouteOptions)
        {
            this.hubRouteOptions = hubRouteOptions.Value;
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var serverManager = serviceProvider.GetRequiredService<NamedPipeServerManager>();
            var dispatcher = serviceProvider.GetRequiredService<NamedPipeConnectionDispatcher>();
            foreach (var hubRoute in hubRouteOptions.HubMap)
            {
                await StartRouteAsync(serverManager, dispatcher, hubRoute);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task StartRouteAsync(NamedPipeServerManager serverManager, NamedPipeConnectionDispatcher dispatcher, KeyValuePair<Uri, Action<IConnectionBuilder>> hubRoute)
        {
            var hubUri = hubRoute.Key;
            var hubConfigure = hubRoute.Value;

            var builder = new ConnectionBuilder(serviceProvider);
            hubConfigure(builder);
            var connection = builder.Build();

            var pipeName = PipeUri.GetAcceptorName(hubUri);

            await serverManager.CreateServerAsync(pipeName, (npc, token) => dispatcher.ExecuteAsync(npc, connection, token)).ConfigureAwait(false);
        }
    }
}

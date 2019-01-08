using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using SignalR.Pipes.Connections;
using SignalR.Pipes.Common;
using SignalR.Pipes.Routing;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;

namespace SignalR.Pipes.Services
{
    internal sealed class SignalRConfigurationService : IHostedService
    {
        private readonly ILogger logger;
        private readonly HostOptions hostOptions;
        private readonly IServiceProvider serviceProvider;
        private NamedPipeServer pipeServer;

        public SignalRConfigurationService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<HostOptions> hostOptions)
        {
            this.serviceProvider = serviceProvider;
            this.hostOptions = hostOptions.Value;
            this.logger = loggerFactory.CreateLogger(typeof(SignalRConfigurationService));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogTrace("Starting...");

            var pipeName = PipeUri.GetAcceptorName(hostOptions.Uri);
            pipeServer = new NamedPipeServer(pipeName, serviceProvider.GetRequiredService<ILoggerFactory>(), BuildRequestPipeline());

            await pipeServer.StartAsync().ConfigureAwait(false);

            logger.LogTrace("Started...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogTrace("Stopping...");

            await (pipeServer?.DisposeAsync() ?? Task.CompletedTask).ConfigureAwait(false);

            logger.LogTrace("Stopped...");
        }

        private Func<NamedPipeServerStream, CancellationToken, Task> BuildRequestPipeline()
        {
            var routeBuilder = new RouteBuilder();
            var connectionsRouteBuilder = new ConnectionsRouteBuilder(serviceProvider, routeBuilder);
            var hubRouteBuilder = new HubRouteBuilder(connectionsRouteBuilder);
            hostOptions.Configure(hubRouteBuilder);

            var routerDelegate = routeBuilder.Build();
            var connectionDispatcher = new NamedPipeConnectionDispatcher(routerDelegate);
            Func<NamedPipeServerStream, CancellationToken, Task> executeDelegate = connectionDispatcher.ExecuteAsync;
            return executeDelegate;
        }
    }
}

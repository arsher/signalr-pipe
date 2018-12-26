using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    public sealed class NamedPipeServerManager
    {
        private readonly IList<NamedPipeServer> namedPipeServers = new List<NamedPipeServer>();
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;

        public NamedPipeServerManager(IApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            applicationLifetime.ApplicationStopping.Register(CloseServers);
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<NamedPipeServerManager>();
        }

        public async Task CreateServerAsync(string pipeName, Func<NamedPipeContext, CancellationToken, Task> onConnected)
        {
            var result = new NamedPipeServer(pipeName, loggerFactory,
                (s, c) => onConnected(new NamedPipeContext(pipeName, s), c));
            await result.StartAsync().ConfigureAwait(false);
        }

        private void CloseServers()
        {
            var tasks = new List<Task>();
            foreach (var server in namedPipeServers)
            {
                tasks.Add(server.DisposeAsync());
            }
            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(5));
        }
    }
}

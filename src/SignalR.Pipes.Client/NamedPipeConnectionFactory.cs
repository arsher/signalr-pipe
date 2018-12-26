using System;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Client
{
    public sealed class NamedPipeConnectionFactory : IConnectionFactory
    {
        private NamedPipeConnectionOptions Options { get; }

        public NamedPipeConnectionFactory(IOptions<NamedPipeConnectionOptions> options)
        {
            this.Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<ConnectionContext> ConnectAsync(TransferFormat transferFormat, CancellationToken cancellationToken = default)
        {
            var connection = new NamedPipeConnection(Options);
            try
            {
                await connection.StartAsync().ConfigureAwait(false);
                return connection;
            }
            catch
            {
                await connection.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        public Task DisposeAsync(ConnectionContext connection)
        {
            return ((NamedPipeConnection)connection).DisposeAsync();
        }
    }
}

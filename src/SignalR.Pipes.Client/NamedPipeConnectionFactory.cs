using System;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Client
{
    /// <summary>
    /// A factory for creating named pipe connection instances.
    /// </summary>
    public sealed class NamedPipeConnectionFactory : IConnectionFactory
    {
        private NamedPipeConnectionOptions Options { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">The connection options.</param>
        public NamedPipeConnectionFactory(IOptions<NamedPipeConnectionOptions> options)
        {
            this.Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task DisposeAsync(ConnectionContext connection)
        {
            return ((NamedPipeConnection)connection).DisposeAsync();
        }
    }
}

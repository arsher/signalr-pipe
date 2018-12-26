using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Client
{
    public sealed class NamedPipeConnection : ConnectionContext, IConnectionInherentKeepAliveFeature
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1, 1);
        private readonly NamedPipeConnectionOptions options;
        private ITransport transport;
        private string connectionId;
        private bool disposed;
        private bool started;

        public override string ConnectionId
        {
            get => connectionId;
            set => throw new InvalidOperationException("The ConnectionId is not used.");
        }

        public override IFeatureCollection Features { get; } = new FeatureCollection();

        public override IDictionary<object, object> Items { get; set; } = new ConnectionItems();

        public override IDuplexPipe Transport
        {
            get
            {
                ThrowIfDisposed();
                if (transport == null)
                {
                    throw new InvalidOperationException($"Cannot access the {nameof(Transport)} pipe before the connection has started.");
                }
                return transport;
            }
            set => throw new NotSupportedException("The transport pipe isn't settable.");
        }

        public bool HasInherentKeepAlive => true;

        public NamedPipeConnection(NamedPipeConnectionOptions options)
        {
            this.options = options;

            Features.Set<IConnectionInherentKeepAliveFeature>(this);
        }

        public async Task StartAsync()
        {
            ThrowIfDisposed();

            if(started)
            {
                return;
            }

            await connectionLock.WaitAsync();
            try
            {
                ThrowIfDisposed();

                if (started)
                {
                    return;
                }

                await CreateTransportAsync();

                started = true;
            }
            finally
            {
                connectionLock.Release();
            }
        }


        public async Task DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            await connectionLock.WaitAsync();
            try
            {
                if (!disposed && started)
                {
                    try
                    {
                        await transport.StopAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            finally
            {
                if (!disposed)
                {
                    disposed = true;
                }

                connectionLock.Release();
            }
        }

        private async Task CreateTransportAsync()
        {
            var transport = new NamedPipeTransport(options);
            try
            {
                connectionId = await transport.StartAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                this.transport = null;
                throw;
            }

            this.transport = transport;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(NamedPipeConnection));
            }
        }
    }
}

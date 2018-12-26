using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SignalR.Pipes.Common.Pipelines;

namespace SignalR.Pipes.Connections
{
    public sealed class NamedPipeConnectionContext : ConnectionContext,
                                                     IConnectionUserFeature,
                                                     IConnectionInherentKeepAliveFeature,
                                                     IConnectionItemsFeature,
                                                     IConnectionIdFeature,
                                                     IConnectionTransportFeature
    { 
        private readonly object _itemsLock = new object();
        private readonly TaskCompletionSource<object> disposeTcs = 
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly string connectionId;
        private readonly NamedPipeServerStream stream;
        private readonly IDuplexPipe transport;
        private readonly ClaimsPrincipal user;
        private IDictionary<object, object> items;

        public override string ConnectionId { get => connectionId; set => throw new InvalidOperationException(""); }

        public override IFeatureCollection Features { get; } = new FeatureCollection();

        public override IDictionary<object, object> Items
        {
            get
            {
                if (items == null)
                {
                    lock (_itemsLock)
                    {
                        if (items == null)
                        {
                            items = new ConnectionItems(new ConcurrentDictionary<object, object>());
                        }
                    }
                }
                return items;
            }
            set => items = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override IDuplexPipe Transport { get => transport; set => new InvalidOperationException(""); }

        public ClaimsPrincipal User { get => user; set => new InvalidOperationException(""); }

        public bool HasInherentKeepAlive => true;

        public NamedPipeConnectionContext(string id, NamedPipeServerStream stream)
        {
            connectionId = id;
            this.stream = stream;
            this.transport = stream.AsDuplexPipe();

            user = ClaimsPrincipal.Current;

            Features.Set<IConnectionTransportFeature>(this);
            Features.Set<IConnectionIdFeature>(this);
            Features.Set<IConnectionUserFeature>(this);
            Features.Set<IConnectionInherentKeepAliveFeature>(this);
            Features.Set<IConnectionItemsFeature>(this);
        }

        public async Task StartAsync()
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 512, true))
            {
                await streamWriter.WriteLineAsync(connectionId).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
                stream.WaitForPipeDrain();
            }
        }
    }
}

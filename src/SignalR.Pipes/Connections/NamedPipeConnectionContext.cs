using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Security.Claims;

namespace SignalR.Pipes.Connections
{
    internal sealed class NamedPipeConnectionContext : ConnectionContext,
                                                       IConnectionUserFeature,
                                                       IConnectionInherentKeepAliveFeature,
                                                       IConnectionItemsFeature,
                                                       IConnectionIdFeature,
                                                       IConnectionTransportFeature
    {
        private readonly object _itemsLock = new object();
        private readonly string connectionId;
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

        public string Name { get; }

        public NamedPipeConnectionContext(string id, string name, IDuplexPipe stream)
        {
            connectionId = id;
            Name = name;
            this.transport = stream;

            user = ClaimsPrincipal.Current;

            Features.Set<IConnectionTransportFeature>(this);
            Features.Set<IConnectionIdFeature>(this);
            Features.Set<IConnectionUserFeature>(this);
            Features.Set<IConnectionInherentKeepAliveFeature>(this);
            Features.Set<IConnectionItemsFeature>(this);
        }
    }
}

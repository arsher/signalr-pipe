using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using SignalR.Pipes.Ipc.Client.Internal;
using System;

namespace SignalR.Pipes.Ipc.Client
{
    public class IpcHubConnection<TClient> : HubConnection, IIpcHubConnection
        where TClient: class
    {
        private readonly IpcHubConnectionState<TClient> state;

        public IpcHubConnection(IConnectionFactory connectionFactory, IHubProtocol protocol,
            IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            TClient client) 
            : base(connectionFactory, protocol, serviceProvider, loggerFactory)
        {
            state = new IpcHubConnectionState<TClient>(this, client);
        }

        public TServer GetServerCore<TServer>() where TServer : class
        {
            return TypedServerBuilder<TServer>.Build(this);
        }
    }
}

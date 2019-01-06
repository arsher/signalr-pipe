using Microsoft.AspNetCore.SignalR;

namespace SignalR.Pipes.Ipc.Internal
{
    public class IpcHubContext<THub, T> : IIpcHubContext<THub, T>
        where THub : Hub<T>
        where T : class
    {
        public IIpcHubClients<T> Clients { get; }

        public IpcHubContext(HubLifetimeManager<THub> lifetimeManager)
        {
            Clients = new IpcHubClients<THub, T>(lifetimeManager);
        }
    }
}

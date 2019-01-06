using Microsoft.AspNetCore.SignalR;

namespace SignalR.Pipes.Ipc
{
    public interface IIpcHubContext<THub, T>
        where THub : Hub<T>
        where T : class
    {
        IIpcHubClients<T> Clients { get; }
    }
}

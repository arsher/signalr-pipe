using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Pipes.Ipc.Client.Internal;
using System;

namespace SignalR.Pipes.Ipc.Client
{
    public static class HubConnectionExtensions
    {
        public static TServer GetServer<TServer>(this HubConnection hubConnection)
            where TServer : class
        {
            if (hubConnection is IIpcHubConnection ipcHubConnection)
            {
                return ipcHubConnection.GetServerCore<TServer>();
            }
            throw new InvalidOperationException();
        }
    }
}

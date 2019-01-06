using System;
using System.Collections.Generic;
using System.Text;

namespace SignalR.Pipes.Ipc.Client.Internal
{
    internal interface IIpcHubConnection
    {
        TServer GetServerCore<TServer>() where TServer : class;
    }
}

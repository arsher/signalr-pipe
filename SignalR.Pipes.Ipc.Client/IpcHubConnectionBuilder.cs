using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Client;

namespace SignalR.Pipes.Ipc.Client
{
    public class IpcHubConnectionBuilder<TClient/*, TServer*/> : NamedPipeHubConnectionBuilder
        where TClient: class
        //where TServer: class
    {
        public IpcHubConnectionBuilder(TClient client)
        {
            Services.AddSingleton<IpcHubConnection<TClient/*, TServer*/>>();
            Services.AddSingleton(client);
        }

        protected override HubConnection BuildInternal(ServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IpcHubConnection<TClient/*, TServer*/>>();
        }
    }
}

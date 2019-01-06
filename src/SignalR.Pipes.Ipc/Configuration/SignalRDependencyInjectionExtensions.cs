using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Ipc.Internal;

namespace SignalR.Pipes.Ipc.Configuration
{
    public static class SignalRDependencyInjectionExtensions
    {
        public static ISignalRServerBuilder AddIpc(this ISignalRServerBuilder builder)
        {
            builder.Services.AddSingleton(typeof(IIpcHubContext<,>), typeof(IpcHubContext<,>));
            return builder;
        }
    }
}

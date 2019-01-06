using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Ipc.Internal
{
    public class IpcHubClients<THub, T> : IIpcHubClients<T> 
        where THub: Hub<T>
        where T : class
    {
        private readonly HubLifetimeManager<THub> lifetimeManager;

        public IpcHubClients(HubLifetimeManager<THub> lifetimeManager)
        {
            this.lifetimeManager = lifetimeManager;
        }

        public T Client(string connectionId)
        {
            return TypedClientBuilder<T>.Build(new InvokeProxy<THub>(lifetimeManager as DefaultHubLifetimeManager<THub>, connectionId));
        }
    }

    internal class InvokeProxy<THub> : IInvokeClientProxy where THub : Hub
    {
        private static readonly FieldInfo ConnectionsField = typeof(DefaultHubLifetimeManager<THub>)
            .GetField("_connections", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly DefaultHubLifetimeManager<THub> lifetimeManager;
        private readonly HubConnectionStore connections;
        private readonly string connectionId;

        public InvokeProxy(DefaultHubLifetimeManager<THub> lifetimeManager, string connectionId)
        {
            this.lifetimeManager = lifetimeManager;
            this.connectionId = connectionId;
            this.connections = ConnectionsField.GetValue(lifetimeManager) as HubConnectionStore;
        }

        public Task<object> InvokeCoreAsync(string method, Type returnType, object[] args, CancellationToken cancellationToken = default)
        {
            var ipcInvocationFeature = connections[connectionId].Features.Get<IIpcInvocationFeature>();
            var invocationId = ipcInvocationFeature.GetNextId();
            var value = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var request = new IpcInvocationRequest(invocationId, returnType, cancellationToken);

            var arguments = args.ToList();
            arguments.Insert(0, invocationId);

            lifetimeManager.SendConnectionAsync(connectionId, method, arguments.ToArray());

            return value.Task;
        }

        public Task InvokeCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            return InvokeCoreAsync(method, null, args, cancellationToken);
        }

        private HubMessage CreateInvocationMessage(string invocationId, string methodName, object[] args)
        {
            return new InvocationMessage(invocationId, methodName, args);
        }
    }
}

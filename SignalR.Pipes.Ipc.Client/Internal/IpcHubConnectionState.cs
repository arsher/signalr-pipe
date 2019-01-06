using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SignalR.Pipes.Ipc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Pipes.Ipc.Client.Internal
{
    internal class IpcHubConnectionState<TClient>
    {
        private readonly HubConnection connection;
        private readonly TClient client;

        public IpcHubConnectionState(HubConnection connection, TClient client)
        {
            this.connection = connection;
            this.client = client;

            Initialize();
        }

        private void Initialize()
        {
            foreach (var clientMethod in typeof(TClient).GetAllInterfaceMethods())
            {
                var parameters = clientMethod.GetParameters().Select(p => p.ParameterType).ToList();
                parameters.Insert(0, typeof(string));

                connection.On(clientMethod.Name, parameters.ToArray(), HandleInvocation, clientMethod);
            }
        }

        private async Task HandleInvocation(object[] args, object state)
        {
            var methodInfo = (MethodInfo)state;
            var invocationId = args.First() as string;
            var arguments = args.Skip(1).ToList();

            var result = methodInfo.Invoke(client, arguments.ToArray()) as Task;

            await result;

            object resultValue = null;
            if (result.GetType().IsGenericType)
            {
                resultValue = result.GetType().GetProperty("Result").GetValue(result);
            }

            await connection.SendAsync("IpcHubResultDone", invocationId, null, JsonConvert.SerializeObject(resultValue), resultValue != null).ConfigureAwait(false);
        }
    }
}

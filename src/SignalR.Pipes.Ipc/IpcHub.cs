using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SignalR.Pipes.Ipc.Internal;

namespace SignalR.Pipes.Ipc
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IpcHub<T> : Hub<T>
        where T : class
    {
        public override Task OnConnectedAsync()
        {
            Context.Features.Set<IIpcInvocationFeature>(new IpcInvocationFeature());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var invocationFeature = Context.Features.Get<IIpcInvocationFeature>();
            invocationFeature.CancelOutstandingInvocations(exception);

            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void IpcHubResultDone(string invocationId, string error, string result, bool hasResult)
        {
            var invocationFeature = Context.Features.Get<IIpcInvocationFeature>();
            if (invocationFeature.TryRemoveInvocation(invocationId, out var invocation))
            {
                var jsonSerializer = new JsonSerializer();
                var reader = new StringReader(result);

                if (!invocation.CancellationToken.IsCancellationRequested)
                {
                    var resultObj = jsonSerializer.Deserialize(new JsonTextReader(reader), invocation.ResultType);
                    invocation.Complete(error, resultObj);
                    invocation.Dispose();
                }
            }
        }
    }
}

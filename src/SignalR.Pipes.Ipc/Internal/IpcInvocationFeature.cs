using System;
using System.Collections.Generic;
using System.Threading;

namespace SignalR.Pipes.Ipc.Internal
{
    internal sealed class IpcInvocationFeature : IIpcInvocationFeature
    {
        private readonly IDictionary<string, IpcInvocationRequest> pendingCalls = new Dictionary<string, IpcInvocationRequest>();
        private int nextId = 0;

        public string GetNextId()
        {
            return Interlocked.Increment(ref nextId).ToString();
        }

        public void AddInvocation(IpcInvocationRequest invocationRequest)
        {
            pendingCalls.Add(invocationRequest.Id, invocationRequest);
        }

        public bool TryGetInvocation(string invocationId, out IpcInvocationRequest request)
        {
            return pendingCalls.TryGetValue(invocationId, out request);
        }

        public bool TryRemoveInvocation(string invocationId, out IpcInvocationRequest request)
        {
            if(pendingCalls.TryGetValue(invocationId, out request))
            {
                pendingCalls.Remove(invocationId);
                return true;
            }
            return false;
        }

        public void CancelOutstandingInvocations(Exception exception)
        {
            foreach (var outstandingCall in pendingCalls.Values)
            {
                if (exception != null)
                {
                    outstandingCall.Fail(exception);
                }
                outstandingCall.Dispose();
            }
            pendingCalls.Clear();
        }
    }
}

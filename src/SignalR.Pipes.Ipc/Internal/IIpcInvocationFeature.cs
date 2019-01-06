using System;

namespace SignalR.Pipes.Ipc.Internal
{
    internal interface IIpcInvocationFeature
    {
        string GetNextId();

        void AddInvocation(IpcInvocationRequest invocationRequest);

        bool TryGetInvocation(string invocationId, out IpcInvocationRequest request);

        bool TryRemoveInvocation(string invocationId, out IpcInvocationRequest request);

        void CancelOutstandingInvocations(Exception exception);
    }
}

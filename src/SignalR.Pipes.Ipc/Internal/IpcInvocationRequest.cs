using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Ipc.Internal
{
    internal sealed class IpcInvocationRequest : IDisposable
    {
        private readonly CancellationTokenRegistration cancellationTokenRegistration;
        private readonly TaskCompletionSource<object> taskCompletion = 
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Id { get; }

        public Type ResultType { get; }

        public CancellationToken CancellationToken { get; }

        public Task<object> Result => taskCompletion.Task;

        public IpcInvocationRequest(string id, Type resultType, CancellationToken cancellationToken)
        {
            cancellationTokenRegistration = cancellationToken.Register(self => ((IpcInvocationRequest)self).Cancel(), this);

            Id = id;
            ResultType = resultType;
            CancellationToken = cancellationToken;
        }

        public void Complete(string error, object result)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Fail(new HubException(error));
                return;
            }

            taskCompletion.TrySetResult(result);
        }

        public void Fail(Exception exception)
        {
            taskCompletion.TrySetException(exception);
        }

        public void Dispose()
        {
            Cancel();
            cancellationTokenRegistration.Dispose();
        }

        private void Cancel()
        {
            taskCompletion.TrySetCanceled();
        }
    }
}

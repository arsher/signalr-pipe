using Microsoft.Extensions.Logging;
using SignalR.Pipes.Common.Messaging;
using SignalR.Pipes.Common.Pipelines;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
#if !NETSTANDARD2_0
using System.Security.AccessControl;
using System.Security.Principal;
using SignalR.Pipes.Internal;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    internal sealed class NamedPipeServer
    {
        public const string PipePrefix = "signalr-";

        private const int MaxNegotiator = 5;
        private const int Timeout = 2000;

        private readonly SemaphoreSlim negotiatorLock = new SemaphoreSlim(MaxNegotiator, MaxNegotiator);
        private readonly SemaphoreSlim stateLock = new SemaphoreSlim(1, 1);
        private readonly TaskCompletionSource<object> startTcs =
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();
#if !NETSTANDARD2_0
        private readonly PipeSecurity pipeSecurity = CreatePipeSecurity();
#endif
        private readonly ILogger logger;
        private readonly Func<NamedPipeServerStream, CancellationToken, Task> onConnected;
        private readonly string pipeName;

        private Task loopTask;
        private bool disposed;
        private bool started;

        public NamedPipeServer(string pipeName, ILoggerFactory loggerFactory,
            Func<NamedPipeServerStream, CancellationToken, Task> onConnected)
        {
            this.pipeName = pipeName;
            this.onConnected = onConnected;
            this.logger = loggerFactory.CreateLogger<NamedPipeServer>();
        }

        public async Task StartAsync()
        {
            ThrowIfDisposed();

            if (started)
            {
                return;
            }

            await stateLock.WaitAsync();
            try
            {
                ThrowIfDisposed();

                if (started)
                {
                    return;
                }

                loopTask = LoopAsync();

                await startTcs.Task;

                started = true;
            }
            finally
            {
                stateLock.Release();
            }
        }

        public async Task DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            await stateLock.WaitAsync();
            try
            {
                if (!disposed && started)
                {
                    try
                    {
                        disposeCts.Cancel();
                        await loopTask.ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        //ignore loop exceptions here
                    }
                }
            }
            finally
            {
                if (!disposed)
                {
                    disposed = true;
                }

                stateLock.Release();
            }
        }

        private async Task LoopAsync()
        {
            try
            {
                while (!disposeCts.IsCancellationRequested)
                {
                    var nextClientTask = ProcessNextClientAsync(disposeCts.Token);
                    if (!startTcs.Task.IsCompleted && !nextClientTask.IsFaulted)
                    {
                        startTcs.TrySetResult(null);
                    }
                    await nextClientTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                startTcs.TrySetCanceled();
            }
            catch (Exception e)
            {
                //needs to run first to avoid deadlock
                if (!startTcs.Task.IsCompleted)
                {
                    startTcs.TrySetException(e);
                }

                await DisposeAsync().ConfigureAwait(false);

                logger.LogError("LoopAsync", e);
            }
        }

        private async Task ProcessNextClientAsync(CancellationToken cancellationToken)
        {
            NamedPipeServerStream pipeStream = null;

            await negotiatorLock.WaitAsync().ConfigureAwait(false);
            try
            {
                pipeStream = CreateNegotiatorStream();
                await pipeStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                var actualPipeName = GetActualPipeName();
                var actualPipe = CreateStream(actualPipeName);

                var pipeWriter = pipeStream.AsPipeWriter();
                pipeWriter.WriteString(actualPipeName);
                var flushResult = await pipeWriter.FlushAsync(disposeCts.Token).ConfigureAwait(false);
                if (!flushResult.IsCompleted)
                {
                    await StartProcessClientAsync(actualPipe);
                }
                else
                {
                    DisposeClosePipe(actualPipe);
                    logger.LogError("Client closed before sending actual pipe name.");
                }
            }
            finally
            {
                DisposeClosePipe(pipeStream);
                negotiatorLock.Release();
            }
        }

        private NamedPipeServerStream CreateStream(string actualPipeName)
        {
#if !NETSTANDARD2_0
            return new NamedPipeServerStream(actualPipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, pipeSecurity, HandleInheritability.None);
#else
            return new NamedPipeServerStream(actualPipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512);
#endif
        }

        private NamedPipeServerStream CreateNegotiatorStream()
        {
#if !NETSTANDARD2_0
            return new NamedPipeServerStream(pipeName, PipeDirection.Out, MaxNegotiator,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, pipeSecurity, HandleInheritability.None);
#else
            return new NamedPipeServerStream(pipeName, PipeDirection.Out, MaxNegotiator,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512);
#endif
        }

        private static void DisposeClosePipe(NamedPipeServerStream pipeStream)
        {
            if (pipeStream?.IsConnected == true)
            {
                pipeStream.Close();
            }
            pipeStream?.Dispose();
        }

        private async Task StartProcessClientAsync(NamedPipeServerStream actualPipe)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, disposeCts.Token))
                {
                    if (!Debugger.IsAttached)
                    {
                        cts.CancelAfter(Timeout);
                    }

                    await actualPipe.WaitForConnectionAsync(linkedCts.Token).ConfigureAwait(false);
                }

                ProcessClientAsync(actualPipe, disposeCts.Token);
            }
            catch (Exception e)
            {
                logger.LogError("StartProcessClientAsync", e);
                DisposeClosePipe(actualPipe);
            }
        }

        private async void ProcessClientAsync(NamedPipeServerStream actualPipe, CancellationToken cancellationToken)
        {
            try
            {
                var connectTask = onConnected?.Invoke(actualPipe, cancellationToken) ?? Task.CompletedTask;
                await connectTask.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError("ProcessClientAsync", e);
            }
            finally
            {
                DisposeClosePipe(actualPipe);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(NamedPipeServer));
            }
        }

        private static string GetActualPipeName()
        {
            return $"{PipePrefix}{Guid.NewGuid():N}";
        }

#if !NETSTANDARD2_0
        private static PipeSecurity CreatePipeSecurity()
        {
            var logonSid = SecurityUtility.GetLogonSidForPid(Process.GetCurrentProcess().Id);
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
                PipeAccessRights.ReadWrite, AccessControlType.Deny));
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.AddAccessRule(new PipeAccessRule(logonSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            return pipeSecurity;
        }
#endif
    }
}

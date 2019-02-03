using Microsoft.Extensions.Logging;
using SignalR.Pipes.Common.Messaging;
using SignalR.Pipes.Common.Pipelines;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    internal sealed partial class NamedPipeServer
    {
        public const string PipePrefix = "signalr-";

        private const int Timeout = 2000;

        private readonly SemaphoreSlim negotiatorLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim stateLock = new SemaphoreSlim(1, 1);
        private readonly TaskCompletionSource<object> startTcs =
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly ILogger logger;
        private readonly Func<NamedPipeServerStream, CancellationToken, Task> onConnected;
        private readonly string pipeName;

        private readonly NamedPipeServerStream negotiatorStream;

        private Task loopTask;
        private bool disposed;
        private bool started;

        public NamedPipeServer(string pipeName, ILoggerFactory loggerFactory,
            Func<NamedPipeServerStream, CancellationToken, Task> onConnected)
        {
            this.pipeName = pipeName;
            this.onConnected = onConnected;
            this.logger = loggerFactory.CreateLogger<NamedPipeServer>();
            negotiatorStream = CreateNegotiatorStream();
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
                Log.LoopError(logger, e);

                //needs to run first to avoid deadlock
                if (!startTcs.Task.IsCompleted)
                {
                    startTcs.TrySetException(e);
                }

                await DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task ProcessNextClientAsync(CancellationToken cancellationToken)
        {
            var pipeWriterCts = new CancellationTokenSource();

            await negotiatorLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await negotiatorStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                Log.ClientConnected(logger);

                var actualPipeName = GetActualPipeName();
                var actualPipe = CreateStream(actualPipeName);

                var pipeWriter = negotiatorStream.AsPipeWriter(pipeWriterCts.Token);
                pipeWriter.WriteString(actualPipeName);
                var flushResult = await pipeWriter.FlushAsync(disposeCts.Token).ConfigureAwait(false);
                if (!flushResult.IsCompleted)
                {
                    await StartProcessClientAsync(actualPipe);
                }
                else
                {
                    DisposeClosePipe(actualPipe);

                    Log.ClientDisconnectedBeforeHandshake(logger);
                }
            }
            finally
            {
                Log.DisconnectingNegotiator(logger);

                pipeWriterCts.Cancel();

                if (negotiatorStream.IsConnected)
                {
                    negotiatorStream.Disconnect();
                }

                negotiatorLock.Release();
            }
        }

        private NamedPipeServerStream CreateStream(string actualPipeName)
        {
            return new NamedPipeServerStream(actualPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512);
        }

        private NamedPipeServerStream CreateNegotiatorStream()
        {
            return new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512);
        }

        private static void DisposeClosePipe(NamedPipeServerStream pipeStream)
        { 
            if (pipeStream?.IsConnected == true)
            {
                pipeStream.Disconnect();
            }
            pipeStream?.Dispose();
        }

        private async Task StartProcessClientAsync(NamedPipeServerStream actualPipe)
        {
            Log.StartingClientConnection(logger);

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
                Log.ErrorWhileStartingClientConnection(logger, e);
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
                Log.ErrorDuringClientProcess(logger, e);
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
    }
}

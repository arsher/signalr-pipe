using System;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using SignalR.Pipes.Common;
using SignalR.Pipes.Common.Messaging;
using SignalR.Pipes.Common.Pipelines;
using PipeOptions = System.IO.Pipes.PipeOptions;

namespace SignalR.Pipes.Client
{
    internal sealed class NamedPipeTransport : ITransport
    {
        private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly NamedPipeConnectionOptions options;
        private NamedPipeClientStream clientStream;
        private IDuplexPipe transport;
        private Task pipeCompletion;

        public PipeReader Input => transport.Input;

        public PipeWriter Output => transport.Output;

        public NamedPipeTransport(NamedPipeConnectionOptions options)
        {
            this.options = options;
        }

        public Task<string> StartAsync()
        {
            return CreateTransportAsync();
        }

        public async Task StopAsync()
        {
            disposeCts.Cancel();
            var completion = pipeCompletion;
            if(completion != null)
            {
                await completion.ConfigureAwait(false);
            }
            clientStream.Dispose();
        }

        private async Task<string> CreateTransportAsync()
        {
            var clientPipe = await InitializeConnectionAsync();
            var connectionId = await HandshakeAsync(clientPipe, disposeCts.Token).ConfigureAwait(false);

            transport = clientPipe;

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            transport.Output.OnReaderCompleted((e, s) => ((TaskCompletionSource<object>)s).TrySetResult(null), tcs);
            transport.Input.OnWriterCompleted((e, s) => ((TaskCompletionSource<object>)s).TrySetResult(null), tcs);

            pipeCompletion = tcs.Task;

            return connectionId;
        }

        private async Task<IDuplexPipe> InitializeConnectionAsync()
        {
            var actualPipeName = await NegotiateActualPipeAsync().ConfigureAwait(false);
            await CreateClientStreamAndConnectAsync(actualPipeName).ConfigureAwait(false);
            var clientPipe = clientStream.AsDuplexPipe(disposeCts.Token);
            return clientPipe;
        }

        private async Task CreateClientStreamAndConnectAsync(string actualPipeName)
        {
            clientStream = new NamedPipeClientStream(".", actualPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await clientStream.ConnectAsync().ConfigureAwait(false);
        }

        private async Task<string> HandshakeAsync(IDuplexPipe pipe, CancellationToken cancellationToken)
        {
            var output = pipe.Output;
            var input = pipe.Input;
            var route = options.Uri.AbsolutePath;

            output.WriteString(route);

            var flushResult = await output.FlushAsync(cancellationToken).ConfigureAwait(false); ;
            if(flushResult.IsCompleted)
            {
                throw new InvalidOperationException("");
            }

            while(true)
            {
                var readResult = await input.ReadAsync(cancellationToken).ConfigureAwait(false);

                var buffer = readResult.Buffer;
                var consumed = buffer.Start;

                try
                {
                    if(!buffer.IsEmpty)
                    {
                        if(TextMessageParser.TryParseStringMessage(ref buffer, out var connectionId))
                        {
                            consumed = buffer.Start;

                            return connectionId;
                        }
                    }
                }
                finally
                {
                    input.AdvanceTo(buffer.Start);
                }
            }
        }

        private async Task<string> NegotiateActualPipeAsync()
        {
            var uri = GetHostUri(options.Uri);
            var pipeName = PipeUri.GetAcceptorName(uri);
            using (var negotiator = new NamedPipeClientStream(".", pipeName, PipeDirection.In, PipeOptions.Asynchronous))
            {
                await negotiator.ConnectAsync();

                using (var reader = new StreamReader(negotiator))
                {
                    return await reader.ReadLineAsync();
                }
            }
        }

        private Uri GetHostUri(Uri uri)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = uri.Scheme;
            uriBuilder.Host = uri.Host;

            return uriBuilder.Uri;
        }
    }
}

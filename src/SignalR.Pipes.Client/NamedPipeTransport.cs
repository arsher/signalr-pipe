using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SignalR.Pipes.Common;
using SignalR.Pipes.Common.Pipelines;
using PipeOptions = System.IO.Pipes.PipeOptions;

namespace SignalR.Pipes.Client
{
    public sealed class NamedPipeTransport : ITransport
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
            var actualPipeName = await NegotiateActualPipeAsync().ConfigureAwait(false);

            clientStream = new NamedPipeClientStream(".", actualPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await clientStream.ConnectAsync().ConfigureAwait(false);

            string result;
            using (var reader = new StreamReader(clientStream, Encoding.UTF8, false, 512, true))
            {
                result = await reader.ReadLineAsync().ConfigureAwait(false);
            }

            transport = clientStream.AsDuplexPipe(disposeCts.Token);

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            transport.Output.OnReaderCompleted((e, s) => ((TaskCompletionSource<object>)s).TrySetResult(null), tcs);
            transport.Input.OnWriterCompleted((e, s) => ((TaskCompletionSource<object>)s).TrySetResult(null), tcs);

            pipeCompletion = tcs.Task;

            return result;
        }

        private async Task<string> NegotiateActualPipeAsync()
        {
            var uri = options.Uri;
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
    }
}

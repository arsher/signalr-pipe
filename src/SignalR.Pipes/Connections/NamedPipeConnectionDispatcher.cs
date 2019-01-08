using SignalR.Pipes.Common.Messaging;
using SignalR.Pipes.Common.Pipelines;
using System;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    internal sealed class NamedPipeConnectionDispatcher
    {
        private static readonly RNGCryptoServiceProvider keyGenerator = new RNGCryptoServiceProvider();

        private readonly Func<NamedPipeConnectionContext, CancellationToken, Task> onConnected;

        public NamedPipeConnectionDispatcher(Func<NamedPipeConnectionContext, CancellationToken, Task> onConnected)
        {
            this.onConnected = onConnected;
        }

        public async Task ExecuteAsync(NamedPipeServerStream stream, CancellationToken cancellationToken)
        {
            var connection = await HandshakeAsync(stream, cancellationToken);
            await onConnected(connection, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<NamedPipeConnectionContext> HandshakeAsync(NamedPipeServerStream stream, CancellationToken cancellationToken)
        {
            var connectionId = MakeNewConnectionId();
            var pipe = stream.AsDuplexPipe(/*cancellationToken*/);
            var hubName = string.Empty;

            try
            {
                while (true)
                {
                    var readResult = await pipe.Input.ReadAsync(cancellationToken).ConfigureAwait(false);

                    var buffer = readResult.Buffer;
                    var consumed = buffer.Start;

                    try
                    {
                        if (!buffer.IsEmpty)
                        {
                            if (TextMessageParser.TryParseStringMessage(ref buffer, out hubName))
                            {
                                consumed = buffer.Start;
                                break;
                            }
                        }

                        if (readResult.IsCompleted)
                        {
                            throw new InvalidOperationException("");
                        }
                    }
                    finally
                    {
                        pipe.Input.AdvanceTo(consumed);
                    }
                }

                pipe.Output.WriteString(connectionId);

                var flushResult = await pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);
                if (flushResult.IsCompleted)
                {
                    throw new InvalidOperationException("");
                }
            }
            catch (Exception e)
            {
                pipe.Input.Complete(e);
                throw;
            }

            var connection = new NamedPipeConnectionContext(connectionId, hubName, pipe);
            return connection;
        }

        private static string MakeNewConnectionId()
        {
            var buffer = new byte[16];
            keyGenerator.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}

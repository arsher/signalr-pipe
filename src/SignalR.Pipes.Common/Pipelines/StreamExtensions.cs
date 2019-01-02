using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Common.Pipelines
{
    internal static class StreamExtensions
    {
        private class DuplexPipe : IDuplexPipe
        {
            internal DuplexPipe(PipeReader input, PipeWriter output)
            {
                Input = input;
                Output = output;
            }

            public PipeReader Input { get; }

            public PipeWriter Output { get; }
        }

        public static IDuplexPipe AsDuplexPipe(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new DuplexPipe(stream.AsPipeReader(cancellationToken), stream.AsPipeWriter(cancellationToken));
        }

        public static PipeReader AsPipeReader(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable", nameof(stream));
            }

            var pipe = new Pipe();
            _ = Task.Run(() => RunReader(stream, pipe, cancellationToken));
            return pipe.Reader;
        }

        public static PipeWriter AsPipeWriter(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable", nameof(stream));
            }

            var pipe = new Pipe();
            _ = Task.Run(() => RunWriter(stream, pipe, cancellationToken));
            return pipe.Writer;
        }

        private static async void RunWriter(Stream stream, Pipe pipe, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var readResult = await pipe.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    if (readResult.Buffer.Length > 0)
                    {
                        foreach (var segment in readResult.Buffer)
                        {
                            await stream.WriteAsync(segment, cancellationToken).ConfigureAwait(false);
                        }

                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }

                    pipe.Reader.AdvanceTo(readResult.Buffer.End);

                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }

                pipe.Reader.Complete();
            }
            catch (Exception ex)
            {
                pipe.Reader.Complete(ex);
                return;
            }
        }

        private static async void RunReader(Stream stream, Pipe pipe, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var memory = pipe.Writer.GetMemory();
                try
                {
                    var bytesRead = await stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    pipe.Writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    pipe.Writer.Complete(ex);
                    return;
                }

                var result = await pipe.Writer.FlushAsync().ConfigureAwait(false);
                if (result.IsCompleted)
                {
                    break;
                }
            }

            pipe.Writer.Complete();
        }


        //copied from corefx
        private static ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask(stream.WriteAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }
            else
            {
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                buffer.Span.CopyTo(sharedBuffer);
                return new ValueTask(FinishWriteAsync(stream.WriteAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer));
            }

            async Task FinishWriteAsync(Task writeTask, byte[] localBuffer)
            {
                try
                {
                    await writeTask.ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(localBuffer);
                }
            }
        }

        //copied from corefx
        private static ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask<int>(stream.ReadAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }
            else
            {
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                return FinishReadAsync(stream.ReadAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer, buffer);

                async ValueTask<int> FinishReadAsync(Task<int> readTask, byte[] localBuffer, Memory<byte> localDestination)
                {
                    try
                    {
                        int result = await readTask.ConfigureAwait(false);
                        new Span<byte>(localBuffer, 0, result).CopyTo(localDestination.Span);
                        return result;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(localBuffer);
                    }
                }
            }
        }
    }
}

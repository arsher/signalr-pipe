using System;
using System.IO;
using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Pipelines;

namespace SignalR.Pipes.Common.Pipelines
{
    public partial class StreamExtensionsTests
    {
        [Fact]
        public void NullStreamThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ((Stream)null).AsDuplexPipe());
        }

        public class Input
        {
            [Fact]
            public void UnreadableStreamThrows()
            {
                var stream = new Mock<Stream>();
                stream.SetupGet(_ => _.CanRead).Returns(false);
                stream.SetupGet(_ => _.CanWrite).Returns(true);
                Assert.Throws<ArgumentException>(() => stream.Object.AsDuplexPipe());
            }

            [Fact]
            public async Task CanBeReadThroughPipe()
            {
                var random = new Random();
                var expectedData = new byte[1024];
                random.NextBytes(expectedData);

                var stream = new MemoryStream(expectedData);
                var duplexPipe = stream.AsDuplexPipe();

                var actualData = new byte[expectedData.Length];
                var actualDataSize = 0;
                while (actualDataSize < expectedData.Length)
                {
                    var result = await duplexPipe.Input.ReadAsync();
                    foreach (var item in result.Buffer)
                    {
                        item.CopyTo(new Memory<byte>(actualData, actualDataSize, actualData.Length - actualDataSize));
                        actualDataSize += item.Length;
                    }
                    duplexPipe.Input.AdvanceTo(result.Buffer.End);
                }

                Assert.Equal(expectedData, actualData);
            }

            [Fact]
            public async Task CompletionReportedOnLastRead()
            {
                var random = new Random();
                var expectedData = new byte[1024];
                random.NextBytes(expectedData);

                var stream = new MemoryStream(expectedData);
                var duplexPipe = stream.AsDuplexPipe();

                var actualData = new byte[expectedData.Length];
                var actualDataSize = 0;
                while (actualDataSize < expectedData.Length)
                {
                    var result = await duplexPipe.Input.ReadAsync();
                    foreach (var item in result.Buffer)
                    {
                        item.CopyTo(new Memory<byte>(actualData, actualDataSize, actualData.Length - actualDataSize));
                        actualDataSize += item.Length;
                    }
                    duplexPipe.Input.AdvanceTo(result.Buffer.End);
                }

                var lastReadResult = await duplexPipe.Input.ReadAsync();

                Assert.True(lastReadResult.IsCompleted);
            }

            [Fact]
            public async Task WriterCompletes()
            {
                var random = new Random();
                var expectedData = new byte[1024];
                random.NextBytes(expectedData);

                var stream = new MemoryStream(expectedData);
                var duplexPipe = stream.AsDuplexPipe();

                var writerTcs = new TaskCompletionSource<object>();
                duplexPipe.Input.OnWriterCompleted((e, o) => writerTcs.TrySetResult(null), null);

                var actualData = new byte[expectedData.Length];
                var actualDataSize = 0;
                while (actualDataSize < expectedData.Length)
                {
                    var result = await duplexPipe.Input.ReadAsync();
                    foreach (var item in result.Buffer)
                    {
                        item.CopyTo(new Memory<byte>(actualData, actualDataSize, actualData.Length - actualDataSize));
                        actualDataSize += item.Length;
                    }
                    duplexPipe.Input.AdvanceTo(result.Buffer.End);
                }

                await writerTcs.Task;
            }

            [Fact]
            public async Task WriterCompletesWithException()
            {
                var streamMock = new Mock<Stream>();
                streamMock.SetupGet(_ => _.CanRead).Returns(true);
                streamMock.SetupGet(_ => _.CanWrite).Returns(true);
                var expectedException = new InvalidOperationException();
                streamMock.Setup(_ => _.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromException<int>(expectedException));

                var duplexPipe = streamMock.Object.AsDuplexPipe();
                var writerTcs = new TaskCompletionSource<object>();
                duplexPipe.Input.OnWriterCompleted((e, o) => writerTcs.TrySetException(e), null);

                await Assert.ThrowsAsync<InvalidOperationException>(() => writerTcs.Task);
            }

            [Fact]
            public async Task ReadAsyncThrows()
            {
                var streamMock = new Mock<Stream>();
                streamMock.SetupGet(_ => _.CanRead).Returns(true);
                streamMock.SetupGet(_ => _.CanWrite).Returns(true);
                var expectedException = new InvalidOperationException();
                streamMock.Setup(_ => _.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromException<int>(expectedException));

                var duplexPipe = streamMock.Object.AsDuplexPipe();

                await Assert.ThrowsAsync<InvalidOperationException>(() => duplexPipe.Input.ReadAsync().AsTask());
            }

            [Fact]
            public async Task CompletesOnReaderCompletion()
            {
                var random = new Random();
                var expectedData = new byte[1024];
                random.NextBytes(expectedData);

                var stream = new MemoryStream(expectedData);
                var duplexPipe = stream.AsDuplexPipe();

                var writerTcs = new TaskCompletionSource<object>();
                duplexPipe.Input.OnWriterCompleted((e, o) => writerTcs.TrySetResult(null), null);

                duplexPipe.Input.Complete();

                await writerTcs.Task;
            }

            [Fact]
            public async Task ReaderCancellationWorks()
            {
                var readTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cts = new CancellationTokenSource();
                cts.Token.Register(() => readTcs.SetCanceled());

                var streamMock = new Mock<Stream>();
                streamMock.SetupGet(_ => _.CanRead).Returns(true);
                streamMock.SetupGet(_ => _.CanWrite).Returns(true);
                streamMock.Setup(_ => _.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), cts.Token))
                    .Returns(readTcs.Task);

                var duplexPipe = streamMock.Object.AsDuplexPipe(cts.Token);

                var readTask = duplexPipe.Input.ReadAsync();

                cts.Cancel();

                var readResult = await readTask.AsTask();

                Assert.True(readResult.IsCompleted);
            }
        }
    }
}

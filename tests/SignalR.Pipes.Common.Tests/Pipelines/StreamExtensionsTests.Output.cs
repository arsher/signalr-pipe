using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SignalR.Pipes.Common.Pipelines
{
    public partial class StreamExtensionsTests
    {
        public class Output
        {
            [Fact]
            public void UnwriteableStreamThrows()
            {
                var stream = new Mock<Stream>();
                stream.SetupGet(_ => _.CanRead).Returns(true);
                stream.SetupGet(_ => _.CanWrite).Returns(false);
                Assert.Throws<ArgumentException>(() => stream.Object.AsDuplexPipe());
            }

            [Fact]
            public async Task CanWriteThroughPipe()
            {
                var random = new Random();
                var expectedData = new byte[1024];
                random.NextBytes(expectedData);

                var stream = new MemoryStream();
                var duplexPipe = stream.AsDuplexPipe();

                await duplexPipe.Output.WriteAsync(expectedData.AsMemory(0, expectedData.Length));
                await duplexPipe.Output.FlushAsync();
                duplexPipe.Output.Complete();

                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                duplexPipe.Output.OnReaderCompleted((e, o) => tcs.SetResult(null), null);

                await tcs.Task;

                Assert.Equal(expectedData, stream.GetBuffer());
            }
        }
    }
}
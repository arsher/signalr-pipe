using System;
using Xunit;

namespace SignalR.Pipes.Common
{
    public class PipeUriTests
    {
        [Fact]
        public void UriValidationThrowsOnWrongScheme()
        {
            Assert.Throws<ArgumentException>(() => PipeUri.Validate(new Uri("http://localhost")));
        }

        [Fact]
        public void UriValidationForSchemeSuccessful()
        {
            PipeUri.Validate(new Uri("signalr.pipe://localhost"));
        }

        [Fact]
        public void ShortUriNameGenerated()
        {
            const string expected = "signalr.pipe_Ec2lnbmFsci5waXBlOi8vTE9DQUxIT1NU";
            var uri = new Uri("signalr.pipe://localhost");

            var actual = PipeUri.GetAcceptorName(uri);
            Assert.Equal(expected, actual);
        }
    }
}

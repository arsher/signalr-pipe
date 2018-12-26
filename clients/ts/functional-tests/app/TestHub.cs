

using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FunctionalTests
{
    public class TestHub : Hub
    {
        public string Echo(string data)
        {
            return data;
        }

        public void ThrowException(string message)
        {
            throw new InvalidOperationException(message);
        }

        public Task InvokeWithString(string message)
        {
            return Clients.Client(Context.ConnectionId).SendAsync("Message", message);
        }

        public ChannelReader<string> Stream()
        {
            var channel = Channel.CreateUnbounded<string>();
            channel.Writer.TryWrite("a");
            channel.Writer.TryWrite("b");
            channel.Writer.TryWrite("c");
            channel.Writer.Complete();
            return channel.Reader;
        }

        public ComplexObject SendComplexObject()
        {
            return new ComplexObject
            {
                ByteArray = new byte[] { 0x1, 0x2, 0x3 },
                DateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                GUID = new Guid("00010203-0405-0607-0706-050403020100"),
                IntArray = new int[] { 1, 2, 3 },
                String = "hello world",
            };
        }
    }
}
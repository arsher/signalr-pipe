using System;
using System.Buffers;
using System.Text;

namespace SignalR.Pipes.Common.Messaging
{
    internal static class TextMessageFormatter
    {
        public static readonly byte RecordSeparator = 0x1e;

        public static void WriteString(this IBufferWriter<byte> @this, string str)
        {
            Span<byte> bytes = Encoding.UTF8.GetBytes(str);
            @this.Write(bytes);
            WriteRecordSeparator(@this);
        }

        public static void WriteRecordSeparator(IBufferWriter<byte> output)
        {
            var buffer = output.GetSpan(1);
            buffer[0] = RecordSeparator;
            output.Advance(1);
        }
    }
}

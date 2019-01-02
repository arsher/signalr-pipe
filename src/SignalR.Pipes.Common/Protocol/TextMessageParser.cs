using System.Buffers;
using System.Text;

namespace SignalR.Pipes.Common.Messaging
{
    internal static class TextMessageParser
    {
        public static bool TryParseStringMessage(ref ReadOnlySequence<byte> buffer, out string payload)
        {
            if(TryParseMessage(ref buffer, out var payloadBuffer))
            {
                payload = Encoding.UTF8.GetString(payloadBuffer.ToArray());
                return true;
            }
            else
            {
                payload = default;
                return false;
            }
        }

        public static bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            var position = buffer.PositionOf(TextMessageFormatter.RecordSeparator);
            if (position == null)
            {
                payload = default;
                return false;
            }

            payload = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }
    }
}

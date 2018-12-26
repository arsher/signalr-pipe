using System.IO.Pipes;

namespace SignalR.Pipes.Connections
{
    public sealed class NamedPipeContext
    {
        public string PipeName { get; }

        public NamedPipeServerStream Stream { get; }

        public NamedPipeContext(string pipeName, NamedPipeServerStream stream)
        {
            PipeName = pipeName;
            Stream = stream;
        }
    }
}

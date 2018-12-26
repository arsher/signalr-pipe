#if !NETSTANDARD2_0
namespace SignalR.Pipes.Internal
{
    internal static partial class Interop
    {
        internal static class Libraries
        {
            public const string Advapi = "advapi32.dll";
            public const string Kernel32 = "kernel32.dll";
        }
    }
}
#endif
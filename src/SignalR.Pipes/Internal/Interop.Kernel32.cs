#if !NETSTANDARD2_0
using System;
using System.Runtime.InteropServices;

namespace SignalR.Pipes.Internal
{
    internal static partial class Interop
    {
        internal static class Kernel32
        {
            public const int PROCESS_QUERY_INFORMATION = 0x0400;

            [DllImport(Libraries.Kernel32, ExactSpelling = true, SetLastError = true)]
            public extern static bool CloseHandle(IntPtr handle);

            [DllImport(Libraries.Kernel32, ExactSpelling = true, SetLastError = true)]
            public static extern SafeCloseHandle OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        }
    }
}
#endif

#if !NETSTANDARD2_0
using Microsoft.Win32.SafeHandles;
using System;

namespace SignalR.Pipes.Internal
{
    internal static partial class Interop
    {
        public sealed class SafeCloseHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            SafeCloseHandle() : base(true) { }
            internal SafeCloseHandle(IntPtr handle, bool ownsHandle)
                : base(ownsHandle)
            {
                SetHandle(handle);
            }

            protected override bool ReleaseHandle()
            {
                return Kernel32.CloseHandle(handle);
            }
        }
    }
}
#endif
#if !NETSTANDARD2_0
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static SignalR.Pipes.Internal.Interop;

namespace SignalR.Pipes.Internal
{
    internal static unsafe class SecurityUtility
    {
        internal static SecurityIdentifier GetLogonSidForPid(int pid)
        {
            var process = OpenProcessForQuery(pid);
            try
            {
                var token = GetProcessToken(process, Advapi.TOKEN_QUERY);
                try
                {
                    var length = GetTokenInformationLength(token, TOKEN_INFORMATION_CLASS.TokenGroups);
                    var tokenInformation = new byte[length];
                    fixed (byte* pTokenInformation = tokenInformation)
                    {
                        GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenGroups, tokenInformation);

                        var ptg = (Advapi.TOKEN_GROUPS*)pTokenInformation;
                        var sids = (Advapi.SID_AND_ATTRIBUTES*)(&(ptg->Groups));

                        for (int i = 0; i < ptg->GroupCount; i++)
                        {
                            if ((sids[i].Attributes & Advapi.SidAttribute.SE_GROUP_LOGON_ID) == Advapi.SidAttribute.SE_GROUP_LOGON_ID)
                            {
                                return new SecurityIdentifier(sids[i].Sid);
                            }
                        }
                    }
                    return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
                }
                finally
                {
                    token.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        private static void GetTokenInformation(SafeCloseHandle token, TOKEN_INFORMATION_CLASS tic, byte[] tokenInformation)
        {
            if (!Advapi.GetTokenInformation(token, tic, tokenInformation, tokenInformation.Length, out int _))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }
        }

        private static int GetTokenInformationLength(SafeCloseHandle token, TOKEN_INFORMATION_CLASS tic)
        {
            var success = Advapi.GetTokenInformation(token, tic, null, 0, out int lengthNeeded);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != Advapi.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw new Win32Exception(error);
                }
            }

            return lengthNeeded;
        }

        private static SafeCloseHandle OpenProcessForQuery(int pid)
        {
            var process = Kernel32.OpenProcess(Kernel32.PROCESS_QUERY_INFORMATION, false, pid);
            if (process.IsInvalid)
            {
                var exception = new Win32Exception();
                process.SetHandleAsInvalid();
                throw exception;
            }
            return process;
        }

        private static SafeCloseHandle GetProcessToken(SafeCloseHandle process, int requiredAccess)
        {
            var success = Advapi.OpenProcessToken(process, requiredAccess, out SafeCloseHandle processToken);
            var error = Marshal.GetLastWin32Error();
            if (!success)
            {
                processToken?.SetHandleAsInvalid();
                throw new Win32Exception(error);
            }

            return processToken;
        }
    }
}
#endif
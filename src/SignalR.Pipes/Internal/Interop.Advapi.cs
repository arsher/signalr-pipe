#if !NETSTANDARD2_0
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SignalR.Pipes.Internal
{
    internal static partial class Interop
    {
        public enum TOKEN_INFORMATION_CLASS : int
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup, 
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            MaxTokenInfoClass
        }

        internal static class Advapi
        {
            public const int ERROR_INSUFFICIENT_BUFFER = 122;
            public const int TOKEN_QUERY = 0x0008;

            [Flags]
            public enum SidAttribute : uint
            {
                SE_GROUP_MANDATORY = 0x1,
                SE_GROUP_ENABLED_BY_DEFAULT = 0x2,
                SE_GROUP_ENABLED = 0x4,
                SE_GROUP_OWNER = 0x8,
                SE_GROUP_USE_FOR_DENY_ONLY = 0x10, // 
                SE_GROUP_RESOURCE = 0x20000000,
                SE_GROUP_LOGON_ID = 0xC0000000,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct SID_AND_ATTRIBUTES
            {
                internal IntPtr Sid;
                internal SidAttribute Attributes;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TOKEN_GROUPS
            {
                internal int GroupCount;
                internal IntPtr Groups; // array of SID_AND_ATTRIBUTES
            }


            [DllImport(Libraries.Advapi, ExactSpelling = true, SetLastError = true)]
            public static extern bool GetTokenInformation(SafeCloseHandle tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, [Out] byte[] pTokenInformation, int tokenInformationLength, out int returnLength);

            [DllImport(Libraries.Advapi, ExactSpelling = true, SetLastError = true)]
            public static extern bool OpenProcessToken(SafeCloseHandle processHandle, int desiredAccess, out SafeCloseHandle tokenHandle);
        }
    }
}
#endif

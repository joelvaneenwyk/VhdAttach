//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using System.Security;

namespace System.ServiceProcess
{
    [
    ComVisible(false),
    SuppressUnmanagedCodeSecurityAttribute
    ]
    internal static class SafeNativeMethods
    {
        [DllImport(ExternDll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, int access);

        [
            DllImport(ExternDll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true),
        ]
        public static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport(ExternDll.Advapi32, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int LsaClose(IntPtr objectHandle);

        [DllImport(ExternDll.Advapi32, SetLastError = false)]
        public static extern int LsaFreeMemory(IntPtr ptr);

        [DllImport(ExternDll.Advapi32, CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int LsaNtStatusToWinError(int ntStatus);

    }
}

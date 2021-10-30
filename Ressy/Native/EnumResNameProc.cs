using System;
using System.Runtime.InteropServices;

namespace Ressy.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
    internal delegate void EnumResNameProc(
        IntPtr hModule,
        IntPtr lpType,
        IntPtr lpName,
        IntPtr lParam
    );
}
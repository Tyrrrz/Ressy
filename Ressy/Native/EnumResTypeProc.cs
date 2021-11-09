using System;
using System.Runtime.InteropServices;

namespace Ressy.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
    internal delegate bool EnumResTypeProc(
        IntPtr hModule,
        IntPtr lpType,
        IntPtr lParam
    );
}
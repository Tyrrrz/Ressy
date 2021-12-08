using System;
using System.Runtime.InteropServices;

namespace Ressy.Native;

[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
internal delegate bool EnumResLangProc(
    IntPtr hModule,
    IntPtr lpType,
    IntPtr lpName,
    ushort langId,
    IntPtr lParam
);
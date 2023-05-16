using System.Runtime.InteropServices;

namespace Ressy.Native;

[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
internal delegate bool EnumResLangProc(
    nint hModule,
    nint lpType,
    nint lpName,
    ushort langId,
    nint lParam
);
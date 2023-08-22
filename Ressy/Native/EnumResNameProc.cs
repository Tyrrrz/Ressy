using System.Runtime.InteropServices;

namespace Ressy.Native;

[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
internal delegate bool EnumResNameProc(nint hModule, nint lpType, nint lpName, nint lParam);

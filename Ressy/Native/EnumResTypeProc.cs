using System.Runtime.InteropServices;

namespace Ressy.Native;

[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
internal delegate bool EnumResTypeProc(nint hModule, nint lpType, nint lParam);

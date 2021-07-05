using System;
using System.Runtime.InteropServices;

namespace Ressy.Native
{
    internal static class NativeHelpers
    {
        public static bool IsIntegerCode(IntPtr handle) => unchecked((ulong) handle.ToInt64()) >> 16 == 0;

        public static string GetString(IntPtr handle) => Marshal.PtrToStringAuto(handle) ?? "";
    }
}
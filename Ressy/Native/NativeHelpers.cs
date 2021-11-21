using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Ressy.Native
{
    internal static class NativeHelpers
    {
        public static string GetString(IntPtr handle) =>
            Marshal.PtrToStringAuto(handle) ??
            throw new Win32Exception($"Pointer {handle} resolves to an empty string.");

        public static void ErrorCheck(Func<bool> invokeNativeMethod)
        {
            if (!invokeNativeMethod())
                throw new Win32Exception();
        }

        public static uint ErrorCheck(Func<uint> invokeNativeMethod)
        {
            var result = invokeNativeMethod();

            if (result <= 0)
                throw new Win32Exception();

            return result;
        }

        public static IntPtr ErrorCheck(Func<IntPtr> invokeNativeMethod)
        {
            var result = invokeNativeMethod();

            if (result == IntPtr.Zero)
                throw new Win32Exception();

            return result;
        }
    }
}
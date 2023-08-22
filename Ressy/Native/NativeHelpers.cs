using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ressy.Native;

internal static class NativeHelpers
{
    public static string GetString(nint handle) =>
        Marshal.PtrToStringAuto(handle)
        ?? throw new Win32Exception($"Pointer {handle} resolved to a null string.");

    public static void ThrowIfError(Func<bool> invokeNativeMethod)
    {
        if (!invokeNativeMethod())
            throw new Win32Exception();
    }

    public static uint ThrowIfError(Func<uint> invokeNativeMethod)
    {
        var result = invokeNativeMethod();

        if (result <= 0)
            throw new Win32Exception();

        return result;
    }

    public static nint ThrowIfError(Func<nint> invokeNativeMethod)
    {
        var result = invokeNativeMethod();

        if (result == 0)
            throw new Win32Exception();

        return result;
    }

    public static void LogIfError(Func<bool> invokeNativeMethod)
    {
        if (!invokeNativeMethod())
        {
            Debug.WriteLine(
                "Win32 error: "
                    + Marshal.GetLastWin32Error()
                    + ". "
                    + "Stacktrace: "
                    + Environment.StackTrace
            );
        }
    }
}

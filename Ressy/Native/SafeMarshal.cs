using System.Runtime.InteropServices;

namespace Ressy.Native;

internal static class SafeMarshal
{
    public static SafeIntPtr AllocHGlobal(int length) =>
        new(Marshal.AllocHGlobal(length), Marshal.FreeHGlobal);

    public static SafeIntPtr AllocHGlobal(byte[] data)
    {
        var ptr = AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);

        return ptr;
    }

    public static SafeIntPtr AllocHGlobal(string data) =>
        new(Marshal.StringToHGlobalAuto(data), Marshal.FreeHGlobal);
}
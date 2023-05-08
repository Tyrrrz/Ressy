using System;
using System.Runtime.InteropServices;

namespace Ressy.Native;

internal partial class NativeMemory : NativeResource
{
    private NativeMemory(IntPtr handle)
        : base(handle)
    {
    }

    protected override void Dispose(bool disposing) =>
        Marshal.FreeHGlobal(Handle);
}

internal partial class NativeMemory
{
    public static NativeMemory Allocate(int length)
    {
        var handle = Marshal.AllocHGlobal(length);
        return new NativeMemory(handle);
    }

    public static NativeMemory Create(byte[] data)
    {
        var memory = Allocate(data.Length);
        Marshal.Copy(data, 0, memory.Handle, data.Length);

        return memory;
    }

    public static NativeMemory Create(string data)
    {
        var handle = Marshal.StringToHGlobalAuto(data);
        return new NativeMemory(handle);
    }
}
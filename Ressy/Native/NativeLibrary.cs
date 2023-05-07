using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Ressy.Native;

internal partial class NativeLibrary : INativeHandle
{
    public IntPtr Handle { get; }

    IntPtr INativeHandle.Value => Handle;

    public NativeLibrary(IntPtr handle) => Handle = handle;

    [ExcludeFromCodeCoverage]
    ~NativeLibrary() => Dispose();

    public void Dispose()
    {
        NativeMethods.FreeLibrary(Handle);
        GC.SuppressFinalize(this);
    }
}

internal partial class NativeLibrary
{
    public static NativeLibrary LoadAsDataFile(string filePath, bool isExclusive = true)
    {
        var handle = NativeMethods.LoadLibraryEx(
            filePath,
            IntPtr.Zero,
            isExclusive ? 0x00000040u : 0x00000002u
        );

        return handle != IntPtr.Zero
            ? new NativeLibrary(handle)
            : throw new Win32Exception();
    }
}
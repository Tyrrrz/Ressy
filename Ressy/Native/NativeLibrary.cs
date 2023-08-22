using System.ComponentModel;

namespace Ressy.Native;

internal partial class NativeLibrary : NativeResource
{
    public NativeLibrary(nint handle)
        : base(handle) { }

    protected override void Dispose(bool disposing) => NativeMethods.FreeLibrary(Handle);
}

internal partial class NativeLibrary
{
    public static NativeLibrary LoadAsDataFile(string filePath, bool isExclusive = true)
    {
        var handle = NativeMethods.LoadLibraryEx(
            filePath,
            0,
            isExclusive ? 0x00000040u : 0x00000002u
        );

        return handle != 0 ? new NativeLibrary(handle) : throw new Win32Exception();
    }
}

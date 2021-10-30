using System;

namespace Ressy.Native
{
    internal partial class NativeLibrary : IDisposable
    {
        public IntPtr Handle { get; }

        public NativeLibrary(IntPtr handle) => Handle = handle;

        ~NativeLibrary() => Dispose();

        public void Dispose()
        {
            // No error check, because we don't want to throw while disposing
            NativeMethods.FreeLibrary(Handle);
        }
    }

    internal partial class NativeLibrary
    {
        public static NativeLibrary Load(string filePath)
        {
            var handle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, 0x00000002)
            );

            return new NativeLibrary(handle);
        }
    }
}
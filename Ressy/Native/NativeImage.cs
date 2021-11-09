using System;
using System.Diagnostics.CodeAnalysis;

namespace Ressy.Native
{
    internal partial class NativeImage : IDisposable
    {
        public IntPtr Handle { get; }

        public NativeImage(IntPtr handle) => Handle = handle;

        [ExcludeFromCodeCoverage]
        ~NativeImage() => Dispose();

        public void Dispose()
        {
            // No error check, because we don't want to throw while disposing
            NativeMethods.FreeLibrary(Handle);
        }
    }

    internal partial class NativeImage
    {
        public static NativeImage Load(string filePath)
        {
            var handle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, 0x00000002)
            );

            return new NativeImage(handle);
        }
    }
}
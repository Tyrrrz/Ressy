using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ressy.Utils
{
    internal class RawUnmanagedMemory : IUnmanagedMemory
    {
        public IntPtr Handle { get; }

        public RawUnmanagedMemory(byte[] data)
        {
            Handle = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Handle, data.Length);
        }

        [ExcludeFromCodeCoverage]
        ~RawUnmanagedMemory() => Dispose();

        public void Dispose() => Marshal.FreeHGlobal(Handle);
    }
}
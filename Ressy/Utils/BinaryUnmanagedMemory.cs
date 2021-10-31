using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ressy.Utils
{
    internal class BinaryUnmanagedMemory : IUnmanagedMemory
    {
        public IntPtr Handle { get; }

        public BinaryUnmanagedMemory(byte[] data)
        {
            Handle = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Handle, data.Length);
        }

        [ExcludeFromCodeCoverage]
        ~BinaryUnmanagedMemory() => Dispose();

        public void Dispose() => Marshal.FreeHGlobal(Handle);
    }
}
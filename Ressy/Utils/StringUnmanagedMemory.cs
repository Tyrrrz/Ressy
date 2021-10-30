using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ressy.Utils
{
    internal class StringUnmanagedMemory : IUnmanagedMemory
    {
        public IntPtr Handle { get; }

        public StringUnmanagedMemory(string value) =>
            Handle = Marshal.StringToHGlobalUni(value);

        [ExcludeFromCodeCoverage]
        ~StringUnmanagedMemory() => Dispose();

        public void Dispose() => Marshal.FreeHGlobal(Handle);
    }
}
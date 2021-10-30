using System;

namespace Ressy.Utils
{
    internal interface IUnmanagedMemory : IDisposable
    {
        IntPtr Handle { get; }
    }
}
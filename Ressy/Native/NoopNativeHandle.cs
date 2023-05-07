using System;

namespace Ressy.Native;

internal class NoopNativeHandle : INativeHandle
{
    public IntPtr Value { get; }

    public NoopNativeHandle(IntPtr value) => Value = value;

    public void Dispose()
    {
    }
}
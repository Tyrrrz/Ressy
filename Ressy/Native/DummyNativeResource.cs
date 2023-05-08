using System;

namespace Ressy.Native;

internal class DummyNativeResource : NativeResource
{
    public DummyNativeResource(IntPtr handle)
        : base(handle)
    {
    }

    protected override void Dispose(bool disposing)
    {
    }
}
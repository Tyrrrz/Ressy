namespace Ressy.Native;

internal class DummyNativeResource : NativeResource
{
    public DummyNativeResource(nint handle)
        : base(handle)
    {
    }

    protected override void Dispose(bool disposing)
    {
    }
}
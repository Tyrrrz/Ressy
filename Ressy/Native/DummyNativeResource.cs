namespace Ressy.Native;

internal class DummyNativeResource(nint handle) : NativeResource(handle)
{
    protected override void Dispose(bool disposing) { }
}

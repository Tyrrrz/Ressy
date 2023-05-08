using System;
using System.Diagnostics.CodeAnalysis;

namespace Ressy.Native;

internal abstract class NativeResource : IDisposable
{
    public IntPtr Handle { get; }

    protected NativeResource(IntPtr handle) => Handle = handle;

    [ExcludeFromCodeCoverage]
    ~NativeResource() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
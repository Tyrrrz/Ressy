using System;
using System.Diagnostics.CodeAnalysis;

namespace Ressy.Native;

internal abstract class NativeResource : IDisposable
{
    public nint Handle { get; }

    protected NativeResource(nint handle) => Handle = handle;

    [ExcludeFromCodeCoverage]
    ~NativeResource() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

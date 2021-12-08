using System;
using System.Diagnostics.CodeAnalysis;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy;

internal partial class ResourceUpdateContext : IDisposable
{
    private readonly SafeIntPtr _handle;

    public ResourceUpdateContext(SafeIntPtr handle) => _handle = handle;

    [ExcludeFromCodeCoverage]
    ~ResourceUpdateContext() => Dispose();

    public void Set(ResourceIdentifier identifier, byte[] data)
    {
        using var typeHandle = identifier.Type.ToPointer();
        using var nameHandle = identifier.Name.ToPointer();
        using var dataHandle = SafeMarshal.AllocHGlobal(data);

        NativeHelpers.ErrorCheck(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeHandle, nameHandle, (ushort)identifier.Language.Id,
                dataHandle, (uint)data.Length
            )
        );
    }

    public void Remove(ResourceIdentifier identifier)
    {
        using var typeHandle = identifier.Type.ToPointer();
        using var nameHandle = identifier.Name.ToPointer();

        NativeHelpers.ErrorCheck(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeHandle, nameHandle, (ushort)identifier.Language.Id,
                IntPtr.Zero, 0
            )
        );
    }

    public void Dispose() => _handle.Dispose();
}

internal partial class ResourceUpdateContext
{
    public static ResourceUpdateContext Create(string imageFilePath, bool deleteExistingResources = false)
    {
        var handle = new SafeIntPtr(
            NativeHelpers.ErrorCheck(() =>
                NativeMethods.BeginUpdateResource(imageFilePath, deleteExistingResources)
            ),
            h => NativeMethods.EndUpdateResource(h, false)
        );

        return new ResourceUpdateContext(handle);
    }
}
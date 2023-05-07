using System;
using System.Diagnostics.CodeAnalysis;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy;

internal partial class ResourceUpdateContext : IDisposable
{
    private readonly IntPtr _handle;

    public ResourceUpdateContext(IntPtr handle) => _handle = handle;

    [ExcludeFromCodeCoverage]
    ~ResourceUpdateContext() => Dispose();

    public void Set(ResourceIdentifier identifier, byte[] data)
    {
        using var typeHandle = identifier.Type.GetHandle();
        using var nameHandle = identifier.Name.GetHandle();
        using var dataMemory = NativeMemory.Create(data);

        NativeHelpers.ThrowIfError(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeHandle.Value, nameHandle.Value, (ushort)identifier.Language.Id,
                dataMemory.Handle, (uint)data.Length
            )
        );
    }

    public void Remove(ResourceIdentifier identifier)
    {
        using var typeHandle = identifier.Type.GetHandle();
        using var nameHandle = identifier.Name.GetHandle();

        NativeHelpers.ThrowIfError(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeHandle.Value, nameHandle.Value, (ushort)identifier.Language.Id,
                IntPtr.Zero, 0
            )
        );
    }

    public void Dispose()
    {
        NativeHelpers.LogIfError(() =>
            NativeMethods.EndUpdateResource(_handle, false)
        );

        // This line is CRITICAL!
        // Attempting to finalize the update context twice leads to really
        // weird errors when calling other resource-related methods later.
        GC.SuppressFinalize(this);
    }
}

internal partial class ResourceUpdateContext
{
    public static ResourceUpdateContext Create(string imageFilePath, bool deleteExistingResources = false)
    {
        var handle = NativeHelpers.ThrowIfError(() =>
            NativeMethods.BeginUpdateResource(imageFilePath, deleteExistingResources)
        );

        return new ResourceUpdateContext(handle);
    }
}
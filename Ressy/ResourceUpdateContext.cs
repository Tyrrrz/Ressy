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
        using var typeMarshaled = identifier.Type.Marshal();
        using var nameMarshaled = identifier.Name.Marshal();
        using var dataMemory = NativeMemory.Create(data);

        NativeHelpers.ThrowIfError(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeMarshaled.Handle, nameMarshaled.Handle, (ushort)identifier.Language.Id,
                dataMemory.Handle, (uint)data.Length
            )
        );
    }

    public void Remove(ResourceIdentifier identifier)
    {
        using var typeMarshaled = identifier.Type.Marshal();
        using var nameMarshaled = identifier.Name.Marshal();

        NativeHelpers.ThrowIfError(() =>
            NativeMethods.UpdateResource(
                _handle,
                typeMarshaled.Handle, nameMarshaled.Handle, (ushort)identifier.Language.Id,
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
using System;
using Ressy.Identification;
using Ressy.Native;
using Ressy.Utils;

// ReSharper disable AccessToDisposedClosure

namespace Ressy
{
    internal partial class ResourceUpdateContext : IResourceUpdateContext, IDisposable
    {
        public IntPtr Handle { get; }

        public ResourceUpdateContext(IntPtr handle) => Handle = handle;

        ~ResourceUpdateContext() => Dispose();

        public void Set(ResourceIdentifier identifier, byte[] data)
        {
            using var typeMemory = identifier.Type.CreateMemory();
            using var nameMemory = identifier.Name.CreateMemory();
            using var dataMemory = new RawUnmanagedMemory(data);

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.UpdateResource(
                    Handle,
                    typeMemory.Handle, nameMemory.Handle, identifier.Language.Id,
                    dataMemory.Handle, (uint)data.Length
                )
            );
        }

        public void Remove(ResourceIdentifier identifier)
        {
            using var typeMemory = identifier.Type.CreateMemory();
            using var nameMemory = identifier.Name.CreateMemory();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.UpdateResource(
                    Handle,
                    typeMemory.Handle, nameMemory.Handle, identifier.Language.Id,
                    IntPtr.Zero, 0
                )
            );
        }

        public void Dispose()
        {
            // No error check, because we don't want to throw while disposing
            NativeMethods.EndUpdateResource(Handle, false);
        }
    }

    internal partial class ResourceUpdateContext
    {
        public static ResourceUpdateContext Create(string filePath, bool deleteExistingResources = false)
        {
            var handle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.BeginUpdateResource(filePath, deleteExistingResources)
            );

            return new ResourceUpdateContext(handle);
        }
    }
}
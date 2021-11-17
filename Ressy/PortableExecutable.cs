using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy
{
    /// <summary>
    /// Portable executable image file.
    /// </summary>
    public class PortableExecutable : IDisposable
    {
        private SafeIntPtr? _handle;

        /// <summary>
        /// Path to the portable executable image file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PortableExecutable"/>.
        /// </summary>
        public PortableExecutable(string filePath) => FilePath = filePath;

        [ExcludeFromCodeCoverage]
        ~PortableExecutable() => Dispose();

        private SafeIntPtr ResolveHandle()
        {
            if (_handle is not null)
                return _handle;

            var handle = new SafeIntPtr(
                NativeHelpers.ErrorCheck(() =>
                    NativeMethods.LoadLibraryEx(FilePath, IntPtr.Zero, 0x00000002)
                ),
                h => NativeMethods.FreeLibrary(h)
            );

            _handle = handle;
            return handle;
        }

        private void ResetHandle()
        {
            _handle?.Dispose();
            _handle = null;
        }

        private IEnumerable<ResourceType> GetResourceTypes()
        {
            var typeHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceTypesEx(
                    ResolveHandle(),
                    (_, typeHandle, _) =>
                    {
                        typeHandles.Add(typeHandle);
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return typeHandles.Select(ResourceType.FromHandle);
        }

        private IEnumerable<ResourceName> GetResourceNames(ResourceType type)
        {
            using var typeHandle = type.ToPointer();

            var nameHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceNamesEx(
                    ResolveHandle(), typeHandle,
                    (_, _, nameHandle, _) =>
                    {
                        nameHandles.Add(nameHandle);
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return nameHandles.Select(ResourceName.FromHandle);
        }

        private IEnumerable<ResourceLanguage> GetResourceLanguages(ResourceType type, ResourceName name)
        {
            using var typeHandle = type.ToPointer();
            using var nameHandle = name.ToPointer();

            var languageIds = new List<ushort>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceLanguagesEx(
                    ResolveHandle(), typeHandle, nameHandle,
                    (_, _, _, languageId, _) =>
                    {
                        languageIds.Add(languageId);
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return languageIds.Select(i => new ResourceLanguage(i));
        }

        /// <summary>
        /// Gets the identifiers of all existing resources.
        /// </summary>
        public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers() =>
        (
            from type in GetResourceTypes()
            from name in GetResourceNames(type)
            from language in GetResourceLanguages(type, name)
            select new ResourceIdentifier(type, name, language)
        ).ToArray();

        /// <summary>
        /// Gets the raw binary data of the specified resource.
        /// Returns <c>null</c> if the resource does not exist.
        /// </summary>
        public Resource? TryGetResource(ResourceIdentifier identifier)
        {
            using var typeHandle = identifier.Type.ToPointer();
            using var nameHandle = identifier.Name.ToPointer();

            var resourceHandle = NativeMethods.FindResourceEx(
                ResolveHandle(),
                typeHandle,
                nameHandle,
                (ushort)identifier.Language.Id
            );

            if (resourceHandle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();

                // Return null if the resource does not exist
                if (error is 1813 or 1814 or 1815)
                    return null;

                // Throw in other cases
                throw new Win32Exception(error);
            }

            var dataHandle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LoadResource(ResolveHandle(), resourceHandle)
            );

            var dataSource = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LockResource(dataHandle)
            );

            var length = NativeHelpers.ErrorCheck(() =>
                NativeMethods.SizeofResource(ResolveHandle(), resourceHandle)
            );

            var data = new byte[length];
            Marshal.Copy(dataSource, data, 0, (int)length);

            return new Resource(identifier, data);
        }

        /// <summary>
        /// Gets the raw binary data of the specified resource.
        /// </summary>
        public Resource GetResource(ResourceIdentifier identifier) =>
            TryGetResource(identifier) ??
            throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

        internal void UpdateResources(Action<ResourceUpdateContext> update, bool deleteExistingResources = false)
        {
            using (var context = ResourceUpdateContext.Create(FilePath, deleteExistingResources))
                update(context);

            // Reset previously obtained handle because the data may have become out of date after changes
            ResetHandle();
        }

        /// <summary>
        /// Removes all existing resources.
        /// </summary>
        public void ClearResources() => UpdateResources(_ => { }, true);

        /// <summary>
        /// Adds or overwrites the specified resource.
        /// </summary>
        public void SetResource(ResourceIdentifier identifier, byte[] data) =>
            UpdateResources(ctx => ctx.Set(identifier, data));

        /// <summary>
        /// Removes the specified resource.
        /// </summary>
        public void RemoveResource(ResourceIdentifier identifier) =>
            UpdateResources(ctx => ctx.Remove(identifier));

        /// <inheritdoc />
        public void Dispose() => ResetHandle();
    }
}
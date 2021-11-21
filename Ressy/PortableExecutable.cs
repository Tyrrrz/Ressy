using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy
{
    /// <summary>
    /// Portable executable image file.
    /// </summary>
    public class PortableExecutable
    {
        /// <summary>
        /// Path to the portable executable image file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PortableExecutable"/>.
        /// </summary>
        public PortableExecutable(string filePath) => FilePath = filePath;

        private SafeIntPtr GetImageHandle()
        {
            var handle = new SafeIntPtr(
                NativeHelpers.ErrorCheck(() =>
                    NativeMethods.LoadLibraryEx(FilePath, IntPtr.Zero, 0x00000040)
                ),
                h => NativeMethods.FreeLibrary(h)
            );

            return handle;
        }

        /// <summary>
        /// Gets the identifiers of all existing resources.
        /// </summary>
        public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
        {
            using var imageHandle = GetImageHandle();

            IReadOnlyList<ResourceType> GetResourceTypes()
            {
                var result = new List<ResourceType>();

                NativeHelpers.ErrorCheck(() =>
                    NativeMethods.EnumResourceTypesEx(
                        imageHandle,
                        (_, typeHandle, _) =>
                        {
                            result.Add(ResourceType.FromHandle(typeHandle));
                            return true;
                        },
                        IntPtr.Zero, 0, 0
                    )
                );

                return result;
            }

            IReadOnlyList<ResourceName> GetResourceNames(ResourceType type)
            {
                using var typeHandle = type.ToPointer();

                var result = new List<ResourceName>();

                NativeHelpers.ErrorCheck(() =>
                    NativeMethods.EnumResourceNamesEx(
                        imageHandle, typeHandle,
                        (_, _, nameHandle, _) =>
                        {
                            result.Add(ResourceName.FromHandle(nameHandle));
                            return true;
                        },
                        IntPtr.Zero, 0, 0
                    )
                );

                return result;
            }

            IReadOnlyList<ResourceLanguage> GetResourceLanguages(ResourceType type, ResourceName name)
            {
                using var typeHandle = type.ToPointer();
                using var nameHandle = name.ToPointer();

                var result = new List<ResourceLanguage>();

                NativeHelpers.ErrorCheck(() =>
                    NativeMethods.EnumResourceLanguagesEx(
                        imageHandle, typeHandle, nameHandle,
                        (_, _, _, languageId, _) =>
                        {
                            result.Add(new ResourceLanguage(languageId));
                            return true;
                        },
                        IntPtr.Zero, 0, 0
                    )
                );

                return result;
            }

            return (
                from type in GetResourceTypes()
                from name in GetResourceNames(type)
                from language in GetResourceLanguages(type, name)
                select new ResourceIdentifier(type, name, language)
            ).ToArray();
        }

        /// <summary>
        /// Gets the raw binary data of the specified resource.
        /// Returns <c>null</c> if the resource does not exist.
        /// </summary>
        public Resource? TryGetResource(ResourceIdentifier identifier)
        {
            using var imageHandle = GetImageHandle();
            using var typeHandle = identifier.Type.ToPointer();
            using var nameHandle = identifier.Name.ToPointer();

            var resourceHandle = NativeMethods.FindResourceEx(
                imageHandle,
                typeHandle,
                nameHandle,
                (ushort)identifier.Language.Id
            );

            if (resourceHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();

                // Return null if the resource does not exist
                if (errorCode is 1813 or 1814 or 1815)
                    return null;

                // Throw in other cases
                throw new Win32Exception(errorCode);
            }

            var dataHandle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LoadResource(imageHandle, resourceHandle)
            );

            var dataSource = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LockResource(dataHandle)
            );

            var length = NativeHelpers.ErrorCheck(() =>
                NativeMethods.SizeofResource(imageHandle, resourceHandle)
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
            using var context = ResourceUpdateContext.Create(FilePath, deleteExistingResources);
            update(context);
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
    }
}
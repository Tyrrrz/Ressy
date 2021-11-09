using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Identification;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy
{
    /// <summary>
    /// Methods for working with resources stored in a portable executable image.
    /// </summary>
    public static partial class PortableExecutable
    {
        private static IEnumerable<ResourceType> GetResourceTypes(IntPtr imageHandle)
        {
            var typeHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceTypesEx(
                    imageHandle,
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

        private static IEnumerable<ResourceName> GetResourceNames(IntPtr imageHandle, ResourceType type)
        {
            using var typeMemory = type.ToUnmanagedMemory();

            var nameHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceNamesEx(
                    imageHandle, typeMemory.Handle,
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

        private static IEnumerable<ResourceLanguage> GetResourceLanguages(
            IntPtr imageHandle,
            ResourceType type,
            ResourceName name)
        {
            using var typeMemory = type.ToUnmanagedMemory();
            using var nameMemory = name.ToUnmanagedMemory();

            var languageIds = new List<ushort>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceLanguagesEx(
                    imageHandle, typeMemory.Handle, nameMemory.Handle,
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
        /// Gets the list of identifiers of all stored resources.
        /// </summary>
        public static IReadOnlyList<ResourceIdentifier> GetResources(string imageFilePath)
        {
            using var image = NativeImage.Load(imageFilePath);

            return (
                from type in GetResourceTypes(image.Handle)
                from name in GetResourceNames(image.Handle, type)
                from language in GetResourceLanguages(image.Handle, type, name)
                select new ResourceIdentifier(type, name, language)
            ).ToArray();
        }

        /// <summary>
        /// Gets the raw binary data of the specified resource.
        /// </summary>
        public static byte[] GetResourceData(string imageFilePath, ResourceIdentifier identifier)
        {
            using var typeMemory = identifier.Type.ToUnmanagedMemory();
            using var nameMemory = identifier.Name.ToUnmanagedMemory();

            using var image = NativeImage.Load(imageFilePath);

            // Resource handle does not need to be freed up
            var resourceHandle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.FindResourceEx(
                    image.Handle,
                    typeMemory.Handle,
                    nameMemory.Handle,
                    identifier.Language.Id
                )
            );

            var dataHandle = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LoadResource(image.Handle, resourceHandle)
            );

            var dataSource = NativeHelpers.ErrorCheck(() =>
                NativeMethods.LockResource(dataHandle)
            );

            var length = NativeHelpers.ErrorCheck(() =>
                NativeMethods.SizeofResource(image.Handle, resourceHandle)
            );

            var data = new byte[length];
            Marshal.Copy(dataSource, data, 0, (int)length);

            return data;
        }

        /// <summary>
        /// Removes all stored resources.
        /// </summary>
        public static void ClearResources(string imageFilePath)
        {
            using var context = ResourceUpdateContext.Create(imageFilePath, true);
        }

        /// <summary>
        /// Adds or overwrites a resource.
        /// </summary>
        public static void SetResource(string imageFilePath, ResourceIdentifier identifier, byte[] data)
        {
            using var context = ResourceUpdateContext.Create(imageFilePath);
            context.Set(identifier, data);
        }

        /// <summary>
        /// Removes a resource.
        /// </summary>
        public static void RemoveResource(string imageFilePath, ResourceIdentifier identifier)
        {
            using var context = ResourceUpdateContext.Create(imageFilePath);
            context.Remove(identifier);
        }
    }
}
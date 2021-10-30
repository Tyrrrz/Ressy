using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Identification;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy
{
    public static class PortableExecutable
    {
        private static IEnumerable<ResourceType> GetResourceTypes(IntPtr imageHandle)
        {
            var typeHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceTypesEx(
                    imageHandle,
                    (_, typeHandle, _) => typeHandles.Add(typeHandle),
                    IntPtr.Zero, 0, 0)
            );

            return typeHandles.Select(ResourceType.FromHandle);
        }

        private static IEnumerable<ResourceName> GetResourceNames(IntPtr imageHandle, ResourceType type)
        {
            using var typeMemory = type.CreateMemory();

            var nameHandles = new List<IntPtr>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceNamesEx(
                    imageHandle, typeMemory.Handle,
                    (_, _, nameHandle, _) => nameHandles.Add(nameHandle),
                    IntPtr.Zero, 0, 0)
            );

            return nameHandles.Select(ResourceName.FromHandle);
        }

        private static IEnumerable<ResourceLanguage> GetResourceLanguages(
            IntPtr imageHandle,
            ResourceType type,
            ResourceName name)
        {
            using var typeMemory = type.CreateMemory();
            using var nameMemory = name.CreateMemory();

            var languageIds = new List<ushort>();

            NativeHelpers.ErrorCheck(() =>
                NativeMethods.EnumResourceLanguagesEx(
                    imageHandle, typeMemory.Handle, nameMemory.Handle,
                    (_, _, _, languageId, _) => languageIds.Add(languageId),
                    IntPtr.Zero, 0, 0)
            );

            return languageIds.Select(i => new ResourceLanguage(i));
        }

        public static IReadOnlyList<ResourceIdentifier> GetResources(string imageFilePath)
        {
            using var image = NativeLibrary.Load(imageFilePath);

            return (
                from type in GetResourceTypes(image.Handle)
                from name in GetResourceNames(image.Handle, type)
                from language in GetResourceLanguages(image.Handle, type, name)
                select new ResourceIdentifier(type, name, language)
            ).ToArray();
        }

        public static byte[] GetResourceData(string imageFilePath, ResourceIdentifier identifier)
        {
            using var typeMemory = identifier.Type.CreateMemory();
            using var nameMemory = identifier.Name.CreateMemory();

            using var image = NativeLibrary.Load(imageFilePath);

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

        public static void UpdateResources(
            string imageFilePath,
            Action<IResourceUpdateContext> update,
            bool clearExistingResources = false)
        {
            using var context = ResourceUpdateContext.Create(imageFilePath, clearExistingResources);
            update(context);
        }

        public static void ClearResources(string imageFilePath) =>
            UpdateResources(imageFilePath, _ => { }, true);
    }
}
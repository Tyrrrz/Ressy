using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Native;

namespace Ressy
{
    public static class PortableExecutable
    {
        private static IEnumerable<ResourceType> GetResourceTypes(IntPtr imageHandle)
        {
            var typeHandles = new List<IntPtr>();

            if (!NativeMethods.EnumResourceTypesEx(imageHandle,
                (_, typeHandle, _) => typeHandles.Add(typeHandle),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return typeHandles.Select(h => new ResourceType(h));
        }

        private static IEnumerable<ResourceName> GetResourceNames(IntPtr imageHandle, ResourceType type)
        {
            var nameHandles = new List<IntPtr>();

            if (!NativeMethods.EnumResourceNamesEx(imageHandle, type.Handle,
                (_, _, nameHandle, _) => nameHandles.Add(nameHandle),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return nameHandles.Select(h => new ResourceName(h));
        }

        private static IEnumerable<ResourceLanguage> GetResourceLanguages(
            IntPtr imageHandle,
            ResourceType type,
            ResourceName name)
        {
            var languageIds = new List<ushort>();

            if (!NativeMethods.EnumResourceLanguagesEx(imageHandle, type.Handle, name.Handle,
                (_, _, _, languageId, _) => languageIds.Add(languageId),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return languageIds.Select(i => new ResourceLanguage(i));
        }

        public static IReadOnlyList<ResourceDescriptor> GetResources(string imageFilePath)
        {
            var moduleHandle = NativeMethods.LoadLibraryEx(imageFilePath, IntPtr.Zero, 0x00000002);
            if (moduleHandle == IntPtr.Zero)
                throw new Win32Exception();

            try
            {
                return (
                    from type in GetResourceTypes(moduleHandle)
                    from name in GetResourceNames(moduleHandle, type)
                    from language in GetResourceLanguages(moduleHandle, type, name)
                    select new ResourceDescriptor(type, name, language)
                ).ToArray();
            }
            finally
            {
                if (!NativeMethods.FreeLibrary(moduleHandle))
                    throw new Win32Exception();
            }
        }

        public static byte[] GetResourceData(string imageFilePath, ResourceDescriptor descriptor)
        {
            var moduleHandle = NativeMethods.LoadLibraryEx(imageFilePath, IntPtr.Zero, 0x00000002);
            if (moduleHandle == IntPtr.Zero)
                throw new Win32Exception();

            try
            {
                // Resource handle does not need to be freed up
                var resourceHandle = NativeMethods.FindResourceEx(
                    moduleHandle,
                    descriptor.Type.Handle,
                    descriptor.Name.Handle,
                    descriptor.Language.Id
                );

                if (resourceHandle == IntPtr.Zero)
                    throw new Win32Exception();

                var dataHandle = NativeMethods.LoadResource(moduleHandle, resourceHandle);
                if (dataHandle == IntPtr.Zero)
                    throw new Win32Exception();

                var dataSource = NativeMethods.LockResource(dataHandle);
                if (dataSource == IntPtr.Zero)
                    throw new Win32Exception();

                var length = NativeMethods.SizeofResource(moduleHandle, resourceHandle);
                if (length <= 0)
                    throw new Win32Exception();

                var data = new byte[length];
                Marshal.Copy(dataSource, data, 0, (int)length);

                return data;
            }
            finally
            {
                if (!NativeMethods.FreeLibrary(moduleHandle))
                    throw new Win32Exception();
            }
        }

        public static void UpdateResources(
            string imageFilePath,
            Action<ResourceUpdateContext> update,
            bool clearExistingResources = false)
        {
            var updateHandle = NativeMethods.BeginUpdateResource(imageFilePath, clearExistingResources);
            if (updateHandle == IntPtr.Zero)
                throw new Win32Exception();

            var context = new ResourceUpdateContext(updateHandle);
            update(context);
            context.Commit();
        }

        public static void ClearResources(string imageFilePath) =>
            UpdateResources(imageFilePath, _ => { }, true);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ressy.Native;

namespace Ressy
{
    public partial class PortableExecutable : IDisposable
    {
        public IntPtr Handle { get; }

        public string FilePath { get; }

        public PortableExecutable(IntPtr handle, string filePath)
        {
            Handle = handle;
            FilePath = filePath;
        }

        ~PortableExecutable() => Dispose();

        public Resource? TryGetResource(ResourceType type, ResourceName name, ResourceLanguage language = default)
        {
            var resourceHandle = NativeMethods.FindResourceEx(Handle, type.Identifier, name.Identifier, language.Id);
            if (resourceHandle == IntPtr.Zero)
                return null;

            return new Resource(
                resourceHandle,
                this,
                type,
                name,
                language
            );
        }

        public Resource GetResource(ResourceType type, ResourceName name, ResourceLanguage language = default) =>
            TryGetResource(type, name, language) ??
            throw new Win32Exception();

        private IReadOnlyList<ResourceType> GetResourceTypes()
        {
            var typeHandles = new List<IntPtr>();

            if (!NativeMethods.EnumResourceTypesEx(Handle,
                (_, typeHandle, _) => typeHandles.Add(typeHandle),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return typeHandles.Select(ResourceType.FromHandle).ToArray();
        }

        private IReadOnlyList<ResourceName> GetResourceNames(ResourceType type)
        {
            var nameHandles = new List<IntPtr>();

            if (!NativeMethods.EnumResourceNamesEx(Handle, type.Identifier,
                (_, _, nameHandle, _) => nameHandles.Add(nameHandle),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return nameHandles.Select(ResourceName.FromHandle).ToArray();
        }

        private IReadOnlyList<ResourceLanguage> GetResourceLanguages(ResourceType type, ResourceName name)
        {
            var languageIds = new List<ushort>();

            if (!NativeMethods.EnumResourceLanguagesEx(Handle, type.Identifier, name.Identifier,
                (_, _, _, languageId, _) => languageIds.Add(languageId),
                IntPtr.Zero, 0, 0))
            {
                throw new Win32Exception();
            }

            return languageIds.Select(i => new ResourceLanguage(i)).ToArray();
        }

        public IReadOnlyList<Resource> GetResources() => (
            from type in GetResourceTypes()
            from name in GetResourceNames(type)
            from language in GetResourceLanguages(type, name)
            select GetResource(type, name, language)
        ).ToArray();

        public void Dispose()
        {
            if (!NativeMethods.FreeLibrary(Handle))
                throw new Win32Exception();
        }
    }

    public partial class PortableExecutable
    {
        public static PortableExecutable FromFile(string filePath)
        {
            var handle = NativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, 0x00000002);
            if (handle == IntPtr.Zero)
                throw new Win32Exception();

            return new PortableExecutable(handle, filePath);
        }
    }
}
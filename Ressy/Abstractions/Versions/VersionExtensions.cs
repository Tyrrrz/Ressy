using System;
using System.IO;
using System.Linq;

namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Extensions for <see cref="PortableExecutable"/> for working with version resources.
    /// </summary>
    public static class VersionExtensions
    {
        private static FileType GetFileType(this PortableExecutable portableExecutable) =>
            Path.GetExtension(portableExecutable.FilePath).ToUpperInvariant() switch
            {
                ".EXE" => FileType.Application,
                ".DLL" => FileType.DynamicallyLinkedLibrary,
                _ => FileType.Unknown
            };

        private static ResourceIdentifier? TryGetVersionInfoResourceIdentifier(
            this PortableExecutable portableExecutable)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.Version.Code)
                .ToArray();

            return
                // Among neutral language resources, find one with the lowest ordinal name (ID)
                identifiers
                    .Where(r => r.Language.Id == ResourceLanguage.Neutral.Id)
                    .Where(r => r.Name.Code is not null)
                    .OrderBy(r => r.Name.Code)
                    .FirstOrDefault() ??
                // If there are no such resources, pick whichever
                identifiers.FirstOrDefault();
        }

        private static Resource? TryGetVersionInfoResource(this PortableExecutable portableExecutable)
        {
            var identifier = portableExecutable.TryGetVersionInfoResourceIdentifier();
            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier);
        }

        /// <summary>
        /// Gets the version info resource and deserializes it.
        /// Returns <c>null</c> if the resource doesn't exist.
        /// </summary>
        /// <remarks>
        /// In case of multiple version info resources, this method retrieves
        /// the one with the lowest ordinal resource name in the neutral language.
        /// If there are no resources matching aforementioned criteria, this method
        /// retrieves the first version info resource it encounters.
        /// </remarks>
        public static VersionInfo? TryGetVersionInfo(this PortableExecutable portableExecutable)
        {
            var resource = portableExecutable.TryGetVersionInfoResource();
            if (resource is null)
                return null;

            return VersionInfo.Deserialize(resource.Data);
        }

        /// <summary>
        /// Gets the version info resource and deserializes it.
        /// </summary>
        /// <remarks>
        /// In case of multiple version info resources, this method retrieves
        /// the one with the lowest ordinal resource name in the neutral language.
        /// If there are no resources matching aforementioned criteria, this method
        /// retrieves the first version info resource it encounters.
        /// </remarks>
        public static VersionInfo GetVersionInfo(this PortableExecutable portableExecutable) =>
            portableExecutable.TryGetVersionInfo() ??
            throw new InvalidOperationException("Version info resource does not exist.");

        /// <summary>
        /// Removes all existing version info resources.
        /// </summary>
        public static void RemoveVersionInfo(this PortableExecutable portableExecutable)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers();

            portableExecutable.UpdateResources(ctx =>
            {
                foreach (var identifier in identifiers)
                {
                    if (identifier.Type.Code == ResourceType.Version.Code)
                        ctx.Remove(identifier);
                }
            });
        }

        /// <summary>
        /// Adds or overwrites a version info resource with the specified data.
        /// </summary>
        public static void SetVersionInfo(
            this PortableExecutable portableExecutable,
            VersionInfo versionInfo)
        {
            var existingResourceIdentifier = portableExecutable.TryGetVersionInfoResourceIdentifier();

            // If the manifest resource already exists, reuse the same identifier
            var identifier =
                existingResourceIdentifier ??
                new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

            portableExecutable.SetResource(identifier, versionInfo.Serialize());
        }

        /// <summary>
        /// Modifies the currently stored version info resource.
        /// If the version info resource doesn't exist, default values will be used as the base instead.
        /// </summary>
        public static void SetVersionInfo(
            this PortableExecutable portableExecutable,
            Action<VersionInfoBuilder> modify)
        {
            var builder = new VersionInfoBuilder();

            var existingResource = portableExecutable.TryGetVersionInfoResource();

            // If the manifest resource already exists, reuse the same identifier
            var identifier =
                existingResource?.Identifier ??
                new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

            // If the manifest resource already exists, reuse the same data as base
            if (existingResource is not null)
            {
                builder.SetAll(VersionInfo.Deserialize(existingResource.Data));
            }
            else
            {
                // Infer reasonable defaults
                builder.SetFileType(portableExecutable.GetFileType());
            }

            modify(builder);

            portableExecutable.SetResource(identifier, builder.Build().Serialize());
        }
    }
}
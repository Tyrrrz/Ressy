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
                ".EXE" => FileType.App,
                ".DLL" => FileType.Dll,
                _ => FileType.Unknown
            };

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
            var identifiers = portableExecutable.GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.Version.Code)
                .ToArray();

            var identifier =
                // Among neutral language resources, find one with the lowest ordinal name (ID)
                identifiers
                    .Where(r => r.Language.Id == ResourceLanguage.Neutral.Id)
                    .Where(r => r.Name.Code is not null)
                    .OrderBy(r => r.Name.Code)
                    .FirstOrDefault() ??
                // If there are no such resources, pick whichever
                identifiers.FirstOrDefault();

            if (identifier is null)
                return null;

            var resource = portableExecutable.TryGetResource(identifier);
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
        /// <remarks>
        /// Consider calling <see cref="RemoveVersionInfo"/> first to remove redundant
        /// manifest resources.
        /// </remarks>
        public static void SetVersionInfo(
            this PortableExecutable portableExecutable,
            VersionInfo versionInfo) =>
            portableExecutable.SetResource(new ResourceIdentifier(
                ResourceType.Version,
                ResourceName.FromCode(1)
            ), versionInfo.Serialize());

        /// <summary>
        /// Adds or overwrites a version info resource based on the existing data.
        /// If the version info resource doesn't exist, a default one is generated automatically.
        /// </summary>
        /// <remarks>
        /// Consider calling <see cref="RemoveVersionInfo"/> first to remove redundant
        /// manifest resources.
        /// </remarks>
        public static void SetVersionInfo(
            this PortableExecutable portableExecutable,
            Action<VersionInfoBuilder> configure)
        {
            var current = portableExecutable.TryGetVersionInfo();

            var builder = current is not null
                ? new VersionInfoBuilder().CopyFrom(current)
                : new VersionInfoBuilder().SetFileType(portableExecutable.GetFileType());

            configure(builder);

            portableExecutable.SetVersionInfo(builder.Build());
        }
    }
}
using System;
using System.IO;
using System.Linq;

namespace Ressy.HighLevel.Versions;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with version resources.
/// </summary>
public static class VersionExtensions
{
    /// <inheritdoc cref="VersionExtensions" />
    extension(Resource resource)
    {
        /// <summary>
        /// Reads the specified resource as a version info resource and
        /// deserializes its data to the corresponding structural representation.
        /// </summary>
        public VersionInfo ReadAsVersionInfo() => VersionInfo.Deserialize(resource.Data);
    }

    /// <inheritdoc cref="VersionExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        private FileType GetFileType() =>
            Path.GetExtension(portableExecutable.FilePath ?? "").ToUpperInvariant() switch
            {
                ".EXE" => FileType.Application,
                ".DLL" => FileType.DynamicallyLinkedLibrary,
                _ => FileType.Unknown,
            };

        private ResourceIdentifier? TryGetVersionInfoResourceIdentifier() =>
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.Version.Code)
                .OrderBy(r => r.Language.Id == Language.Neutral.Id)
                .ThenBy(r => r.Name.Code ?? int.MaxValue)
                .FirstOrDefault();

        private Resource? TryGetVersionInfoResource()
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
        /// If there are multiple version info resources, this method retrieves the one
        /// with the lowest ordinal name (ID), giving preference to resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no matching resources, this method retrieves the first
        /// version info resource it finds.
        /// </remarks>
        public VersionInfo? TryGetVersionInfo() =>
            portableExecutable.TryGetVersionInfoResource()?.ReadAsVersionInfo();

        /// <summary>
        /// Gets the version info resource and deserializes it.
        /// </summary>
        /// <remarks>
        /// In case of multiple version info resources, this method retrieves
        /// the one with the lowest ordinal resource name in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no resources matching aforementioned criteria, this method
        /// retrieves the first version info resource it encounters.
        /// </remarks>
        public VersionInfo GetVersionInfo() =>
            portableExecutable.TryGetVersionInfo()
            ?? throw new InvalidOperationException("Version info resource does not exist.");

        /// <summary>
        /// Removes all existing version info resources.
        /// </summary>
        public void RemoveVersionInfo() =>
            portableExecutable.RemoveResources(r => r.Type.Code == ResourceType.Version.Code);

        /// <summary>
        /// Adds or overwrites a version info resource with the specified data.
        /// </summary>
        /// <remarks>
        /// If a version info resource already exists (based on <see cref="TryGetVersionInfo" /> rules),
        /// its identifier will be reused for the new resource.
        /// If no version info resource exists, a new one will be created with
        /// an ordinal name (ID) of 1 in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public void SetVersionInfo(VersionInfo versionInfo)
        {
            // If the resource already exists, reuse the same identifier
            var identifier =
                portableExecutable.TryGetVersionInfoResourceIdentifier()
                ?? new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

            portableExecutable.SetResource(new Resource(identifier, versionInfo.Serialize()));
        }

        /// <summary>
        /// Modifies the currently stored version info resource.
        /// </summary>
        /// <remarks>
        /// If a version info resource already exists (based on <see cref="TryGetVersionInfo" /> rules),
        /// it will be updated with the new data while keeping the same identifier.
        /// If no version info resource exists, a new one will be created with
        /// an ordinal name (ID) of 1 in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public void SetVersionInfo(Action<VersionInfoBuilder> modify)
        {
            var builder = new VersionInfoBuilder();

            var existingResource = portableExecutable.TryGetVersionInfoResource();

            // If the resource already exists, reuse the same identifier
            var identifier =
                existingResource?.Identifier
                ?? new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

            // If the resource already exists, use the data as base
            if (existingResource is not null)
            {
                builder.SetAll(existingResource.ReadAsVersionInfo());
            }
            // Otherwise, infer reasonable defaults as base
            else
            {
                builder.SetFileType(portableExecutable.GetFileType());
            }

            modify(builder);

            portableExecutable.SetResource(new Resource(identifier, builder.Build().Serialize()));
        }
    }
}

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
            Path.GetExtension(portableExecutable.FilePath).ToUpperInvariant() switch
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
        /// Returns <c>null</c> if the resource doesn't exist or can't be deserialized.
        /// </summary>
        /// <remarks>
        /// If there are multiple version info resources, this method retrieves the one
        /// with the lowest ordinal name (ID), giving preference to resources
        /// in the neutral language.
        /// If there are no matching resources, this method retrieves the first
        /// version info resource it finds.
        /// </remarks>
        public VersionInfo? TryGetVersionInfo()
        {
            var resource = portableExecutable.TryGetVersionInfoResource();
            if (resource is null)
                return null;

            try
            {
                return resource.ReadAsVersionInfo();
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is InvalidDataException)
            {
                return null;
            }
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
        public VersionInfo GetVersionInfo() =>
            portableExecutable.TryGetVersionInfo()
            ?? throw new InvalidOperationException("Version info resource does not exist.");

        /// <summary>
        /// Removes all existing version info resources.
        /// </summary>
        public void RemoveVersionInfo()
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
        public void SetVersionInfo(VersionInfo versionInfo)
        {
            // If the resource already exists, reuse the same identifier
            var identifier =
                portableExecutable.TryGetVersionInfoResourceIdentifier()
                ?? new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

            portableExecutable.SetResource(identifier, versionInfo.Serialize());
        }

        /// <summary>
        /// Modifies the currently stored version info resource.
        /// If the version info resource doesn't exist, default values will be used
        /// for properties that haven't been provided.
        /// </summary>
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

            portableExecutable.SetResource(identifier, builder.Build().Serialize());
        }
    }
}

using System;
using System.IO;
using System.Linq;

namespace Ressy.HighLevel.Versions;

/// <summary>
/// Extensions for <see cref="PortableExecutable"/> for working with version resources.
/// </summary>
public static class VersionExtensions
{
    /// <summary>
    /// Reads the specified resource as as a version info resource and
    /// deserializes its data to the corresponding structural representation.
    /// </summary>
    public static VersionInfo ReadAsVersionInfo(this Resource resource) =>
        VersionInfo.Deserialize(resource.Data);

    private static FileType GetFileType(this PortableExecutable portableExecutable) =>
        Path.GetExtension(portableExecutable.FilePath).ToUpperInvariant() switch
        {
            ".EXE" => FileType.Application,
            ".DLL" => FileType.DynamicallyLinkedLibrary,
            _ => FileType.Unknown
        };

    private static ResourceIdentifier? TryGetVersionInfoResourceIdentifier(this PortableExecutable portableExecutable) =>
        portableExecutable.GetResourceIdentifiers()
            .Where(r => r.Type.Code == ResourceType.Version.Code)
            .OrderBy(r => r.Language.Id == Language.Neutral.Id)
            .ThenBy(r => r.Name.Code ?? int.MaxValue)
            .FirstOrDefault();

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
    /// If there are multiple version info resources, this method retrieves the one
    /// with the lowest ordinal name (ID), while giving preference to resources
    /// in the neutral language.
    /// If there are no matching resources, this method retrieves the first
    /// version info resource it finds.
    /// </remarks>
    public static VersionInfo? TryGetVersionInfo(this PortableExecutable portableExecutable) =>
        portableExecutable.TryGetVersionInfoResource()?.ReadAsVersionInfo();

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

        // If the resource already exists, reuse the same identifier
        var identifier =
            existingResourceIdentifier ??
            new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

        portableExecutable.SetResource(identifier, versionInfo.Serialize());
    }

    /// <summary>
    /// Modifies the currently stored version info resource.
    /// If the version info resource doesn't exist, default values will be used
    /// for properties that haven't been provided.
    /// </summary>
    public static void SetVersionInfo(
        this PortableExecutable portableExecutable,
        Action<VersionInfoBuilder> modify)
    {
        var builder = new VersionInfoBuilder();

        var existingResource = portableExecutable.TryGetVersionInfoResource();

        // If the resource already exists, reuse the same identifier
        var identifier =
            existingResource?.Identifier ??
            new ResourceIdentifier(ResourceType.Version, ResourceName.FromCode(1));

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
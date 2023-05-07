using System;
using System.Linq;
using System.Text;

namespace Ressy.HighLevel.Manifests;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with manifest resources.
/// </summary>
public static class ManifestExtensions
{
    // Unlike most string resources, manifests are encoded in UTF-8,
    // because that's the default encoding for XML files.
    private static readonly Encoding DefaultManifestEncoding = Encoding.UTF8;

    /// <summary>
    /// Reads the specified resource as as a manifest resource and
    /// decodes its data to an XML text string.
    /// </summary>
    public static string ReadAsManifest(this Resource resource, Encoding? encoding = null) =>
        resource.ReadAsString(encoding ?? DefaultManifestEncoding);

    private static ResourceIdentifier? TryGetManifestResourceIdentifier(this PortableExecutable portableExecutable) =>
        portableExecutable.GetResourceIdentifiers()
            .Where(r => r.Type.Code == ResourceType.Manifest.Code)
            .OrderBy(r => r.Language.Id == Language.Neutral.Id)
            .ThenBy(r => r.Name.Code ?? int.MaxValue)
            .FirstOrDefault();

    private static Resource? TryGetManifestResource(this PortableExecutable portableExecutable)
    {
        var identifier = portableExecutable.TryGetManifestResourceIdentifier();
        if (identifier is null)
            return null;

        return portableExecutable.TryGetResource(identifier);
    }

    /// <summary>
    /// Gets the manifest resource and reads its data as an XML text string.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    /// <remarks>
    /// If there are multiple manifest resources, this method retrieves the one
    /// with the lowest ordinal name (ID), giving preference to resources
    /// in the neutral language.
    /// If there are no matching resources, this method retrieves the first
    /// manifest resource it finds.
    /// </remarks>
    public static string? TryGetManifest(this PortableExecutable portableExecutable, Encoding? encoding = null) =>
        portableExecutable.TryGetManifestResource()?.ReadAsManifest(encoding);

    /// <summary>
    /// Gets the manifest resource and reads its data as an XML text string.
    /// </summary>
    /// <remarks>
    /// If there are multiple manifest resources, this method retrieves the one
    /// with the lowest ordinal name (ID), giving preference to resources
    /// in the neutral language.
    /// If there are no matching resources, this method retrieves the first
    /// manifest resource it finds.
    /// </remarks>
    public static string GetManifest(this PortableExecutable portableExecutable, Encoding? encoding = null) =>
        portableExecutable.TryGetManifest(encoding) ??
        throw new InvalidOperationException("Application manifest resource does not exist.");

    /// <summary>
    /// Removes all existing manifest resources.
    /// </summary>
    public static void RemoveManifest(this PortableExecutable portableExecutable)
    {
        var identifiers = portableExecutable.GetResourceIdentifiers();

        portableExecutable.UpdateResources(ctx =>
        {
            foreach (var identifier in identifiers)
            {
                if (identifier.Type.Code == ResourceType.Manifest.Code)
                    ctx.Remove(identifier);
            }
        });
    }

    /// <summary>
    /// Adds or overwrites an manifest resource with the specified XML text string.
    /// </summary>
    public static void SetManifest(
        this PortableExecutable portableExecutable,
        string manifest,
        Encoding? encoding = null)
    {
        // If the resource already exists, reuse the same identifier
        var identifier =
            portableExecutable.TryGetManifestResourceIdentifier() ??
            new ResourceIdentifier(ResourceType.Manifest, ResourceName.FromCode(1));

        portableExecutable.SetResource(identifier, (encoding ?? DefaultManifestEncoding).GetBytes(manifest));
    }
}
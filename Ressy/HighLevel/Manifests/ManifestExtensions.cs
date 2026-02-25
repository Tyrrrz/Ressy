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

    /// <inheritdoc cref="ManifestExtensions" />
    extension(Resource resource)
    {
        /// <summary>
        /// Reads the specified resource as a manifest resource and
        /// decodes its data to an XML text string.
        /// </summary>
        public string ReadAsManifest(Encoding? encoding = null) =>
            resource.ReadAsString(encoding ?? DefaultManifestEncoding);
    }

    /// <inheritdoc cref="ManifestExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        private ResourceIdentifier? TryGetManifestResourceIdentifier() =>
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.Manifest.Code)
                .OrderBy(r => r.Language.Id == Language.Neutral.Id)
                .ThenBy(r => r.Name.Code ?? int.MaxValue)
                .FirstOrDefault();

        private Resource? TryGetManifestResource()
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
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no matching resources, this method retrieves the first
        /// manifest resource it finds.
        /// </remarks>
        public string? TryGetManifest(Encoding? encoding = null) =>
            portableExecutable.TryGetManifestResource()?.ReadAsManifest(encoding);

        /// <summary>
        /// Gets the manifest resource and reads its data as an XML text string.
        /// </summary>
        /// <remarks>
        /// If there are multiple manifest resources, this method retrieves the one
        /// with the lowest ordinal name (ID), giving preference to resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no matching resources, this method retrieves the first
        /// manifest resource it finds.
        /// </remarks>
        public string GetManifest(Encoding? encoding = null) =>
            portableExecutable.TryGetManifest(encoding)
            ?? throw new InvalidOperationException("Application manifest resource does not exist.");

        /// <summary>
        /// Removes all existing manifest resources.
        /// </summary>
        public void RemoveManifest() =>
            portableExecutable.RemoveResources(r => r.Type.Code == ResourceType.Manifest.Code);

        /// <summary>
        /// Adds or overwrites a manifest resource with the specified XML text string.
        /// </summary>
        /// <remarks>
        /// If a manifest resource already exists (based on <see cref="TryGetManifest" /> rules),
        /// its identifier will be reused for the new resource.
        /// If no manifest resource exists, a new one will be created with
        /// an ordinal name (ID) of 1 in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public void SetManifest(string manifest, Encoding? encoding = null)
        {
            // If the resource already exists, reuse the same identifier
            var identifier =
                portableExecutable.TryGetManifestResourceIdentifier()
                ?? new ResourceIdentifier(ResourceType.Manifest, ResourceName.FromCode(1));

            portableExecutable.SetResource(
                new Resource(identifier, (encoding ?? DefaultManifestEncoding).GetBytes(manifest))
            );
        }
    }
}

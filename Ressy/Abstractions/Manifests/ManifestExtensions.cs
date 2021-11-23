using System;
using System.Linq;
using System.Text;

namespace Ressy.Abstractions.Manifests
{
    /// <summary>
    /// Extensions for <see cref="PortableExecutable"/> for working with manifest resources.
    /// </summary>
    public static class ManifestExtensions
    {
        // Unlike most string resources, manifests are encoded in UTF-8,
        // because that's the default encoding for XML files.
        private static readonly Encoding DefaultManifestEncoding = Encoding.UTF8;

        /// <summary>
        /// Gets the manifest resource and reads its data as an XML text string.
        /// Returns <c>null</c> if the resource doesn't exist.
        /// </summary>
        /// <remarks>
        /// In case of multiple manifest resources, this method retrieves
        /// the one with the lowest ordinal resource name in the neutral language.
        /// If there are no resources matching aforementioned criteria, this method
        /// retrieves the first manifest resource it encounters.
        /// </remarks>
        public static string? TryGetManifest(this PortableExecutable portableExecutable, Encoding? encoding = null)
        {
            var identifiers = portableExecutable.GetResourceIdentifiers()
                .Where(r => r.Type.Code == ResourceType.Manifest.Code)
                .ToArray();

            var identifier =
                // Among neutral language resources, find the one with the lowest ordinal name (ID)
                identifiers
                    .Where(r => r.Language.Id == ResourceLanguage.Neutral.Id)
                    .Where(r => r.Name.Code is not null)
                    .OrderBy(r => r.Name.Code)
                    .FirstOrDefault() ??
                // If there are no such resources, pick whichever
                identifiers.FirstOrDefault();

            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier)?.ReadAsString(encoding ?? DefaultManifestEncoding);
        }

        /// <summary>
        /// Gets the manifest resource and reads its data as an XML text string.
        /// </summary>
        /// <remarks>
        /// In case of multiple manifest resources, this method retrieves
        /// the one with the lowest ordinal resource name in the neutral language.
        /// If there are no resources matching aforementioned criteria, this method
        /// retrieves the first manifest resource it encounters.
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
        /// <remarks>
        /// Consider calling <see cref="RemoveManifest"/> first to remove redundant
        /// manifest resources.
        /// </remarks>
        public static void SetManifest(
            this PortableExecutable portableExecutable,
            string manifest,
            Encoding? encoding = null)
        {
            portableExecutable.SetResource(new ResourceIdentifier(
                ResourceType.Manifest,
                ResourceName.FromCode(1)
            ), (encoding ?? DefaultManifestEncoding).GetBytes(manifest));
        }
    }
}
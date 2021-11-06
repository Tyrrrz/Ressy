using System.Text;
using Ressy.Identification;

namespace Ressy
{
    public partial class PortableExecutable
    {
        /// <summary>
        /// Gets the content of an application manifest resource.
        /// </summary>
        public static string GetApplicationManifest(string imageFilePath, ResourceLanguage language) =>
            Encoding.UTF8.GetString(
                GetResourceData(
                    imageFilePath,
                    new ResourceIdentifier(
                        ResourceType.FromCode(StandardResourceTypeCode.Manifest),
                        ResourceName.FromCode(1),
                        language
                    )
                )
            );

        /// <summary>
        /// Gets the content of an application manifest resource.
        /// </summary>
        public static string GetApplicationManifest(string imageFilePath) =>
            GetApplicationManifest(imageFilePath, ResourceLanguage.Neutral);

        /// <summary>
        /// Sets the content of an application manifest resource.
        /// </summary>
        public static void SetApplicationManifest(string imageFilePath, string manifest, ResourceLanguage language) =>
            SetResource(
                imageFilePath,
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Manifest),
                    ResourceName.FromCode(1),
                    language
                ),
                Encoding.UTF8.GetBytes(manifest)
            );

        /// <summary>
        /// Sets the content of an application manifest resource.
        /// </summary>
        public static void SetApplicationManifest(string imageFilePath, string manifest) =>
            SetApplicationManifest(imageFilePath, manifest, ResourceLanguage.Neutral);

        /// <summary>
        /// Removes existing application manifest resource.
        /// </summary>
        public static void RemoveApplicationManifest(string imageFilePath, ResourceLanguage language) =>
            RemoveResource(
                imageFilePath,
                new ResourceIdentifier(
                    ResourceType.FromCode(StandardResourceTypeCode.Manifest),
                    ResourceName.FromCode(1),
                    language
                )
            );

        /// <summary>
        /// Removes existing application manifest resource.
        /// </summary>
        public static void RemoveApplicationManifest(string imageFilePath) =>
            RemoveApplicationManifest(imageFilePath, ResourceLanguage.Neutral);
    }
}
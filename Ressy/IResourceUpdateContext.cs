using Ressy.Identification;

namespace Ressy
{
    /// <summary>
    /// Represents a deferred update of resources stored in a portable executable image.
    /// </summary>
    public interface IResourceUpdateContext
    {
        /// <summary>
        /// Adds or overwrites a resource.
        /// </summary>
        void Set(ResourceIdentifier identifier, byte[] data);

        /// <summary>
        /// Removes a resource.
        /// </summary>
        void Remove(ResourceIdentifier identifier);
    }
}
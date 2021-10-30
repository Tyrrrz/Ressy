using Ressy.Identification;

namespace Ressy
{
    public interface IResourceUpdateContext
    {
        void Set(ResourceIdentifier identifier, byte[] data);

        void Remove(ResourceIdentifier identifier);
    }
}
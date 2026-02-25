using System;
using System.Linq;

namespace Ressy.MultilingualUserInterface;

/// <summary>
/// Extensions for <see cref="PortableExecutable" /> for working with MUI resources.
/// </summary>
// https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
public static class MuiExtensions
{
    /// <inheritdoc cref="MuiExtensions" />
    extension(Resource resource)
    {
        /// <summary>
        /// Reads the specified resource as a MUI resource and
        /// deserializes its data to the corresponding structural representation.
        /// </summary>
        public MuiInfo ReadAsMuiInfo() => MuiInfo.Deserialize(resource.Data);
    }

    /// <inheritdoc cref="MuiExtensions" />
    extension(PortableExecutable portableExecutable)
    {
        private ResourceIdentifier? TryGetMuiResourceIdentifier() =>
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Equals(ResourceType.Mui))
                .OrderBy(r => r.Language.Id == Language.Neutral.Id)
                .ThenBy(r => r.Name.Code ?? int.MaxValue)
                .FirstOrDefault();

        private Resource? TryGetMuiResource()
        {
            var identifier = portableExecutable.TryGetMuiResourceIdentifier();
            if (identifier is null)
                return null;

            return portableExecutable.TryGetResource(identifier);
        }

        /// <summary>
        /// Gets the MUI resource and deserializes it.
        /// Returns <c>null</c> if the resource doesn't exist.
        /// </summary>
        /// <remarks>
        /// If there are multiple MUI resources, this method retrieves the one
        /// with the lowest ordinal name (ID), giving preference to resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no matching resources, this method retrieves the first
        /// MUI resource it finds.
        /// </remarks>
        public MuiInfo? TryGetMuiInfo() => portableExecutable.TryGetMuiResource()?.ReadAsMuiInfo();

        /// <summary>
        /// Gets the MUI resource and deserializes it.
        /// </summary>
        /// <remarks>
        /// If there are multiple MUI resources, this method retrieves the one
        /// with the lowest ordinal name (ID), giving preference to resources
        /// in the neutral language (<see cref="Language.Neutral" />).
        /// If there are no matching resources, this method retrieves the first
        /// MUI resource it finds.
        /// </remarks>
        public MuiInfo GetMuiInfo() =>
            portableExecutable.TryGetMuiInfo()
            ?? throw new InvalidOperationException("MUI resource does not exist.");

        /// <summary>
        /// Removes all existing MUI resources.
        /// </summary>
        public void RemoveMuiInfo() =>
            portableExecutable.RemoveResources(r => r.Type.Equals(ResourceType.Mui));

        /// <summary>
        /// Adds or overwrites a MUI resource with the specified data.
        /// </summary>
        /// <remarks>
        /// If a MUI resource already exists (based on <see cref="TryGetMuiInfo" /> rules),
        /// its identifier will be reused for the new resource.
        /// If no MUI resource exists, a new one will be created with
        /// an ordinal name (ID) of 1 in the neutral language (<see cref="Language.Neutral" />).
        /// </remarks>
        public void SetMuiInfo(MuiInfo muiInfo)
        {
            // If the resource already exists, reuse the same identifier
            var identifier =
                portableExecutable.TryGetMuiResourceIdentifier()
                ?? new ResourceIdentifier(ResourceType.Mui, ResourceName.FromCode(1));

            portableExecutable.SetResource(new Resource(identifier, muiInfo.Serialize()));
        }
    }
}

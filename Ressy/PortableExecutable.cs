using System;
using System.Collections.Generic;
using System.Linq;
using Ressy.PE;

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
public class PortableExecutable(string filePath)
{
    /// <summary>
    /// Path to the portable executable image file.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers() =>
        PeFile.ReadResourceIdentifiers(FilePath);

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    public IReadOnlyList<Resource> GetResources() => PeFile.ReadResources(FilePath);

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        var data = PeFile.TryReadResource(FilePath, identifier);
        return data is not null ? new Resource(identifier, data) : null;
    }

    /// <summary>
    /// Gets the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    /// <summary>
    /// Adds or overwrites the specified resources.
    /// If <paramref name="removeOthers"/> is <c>true</c>, all existing resources not present
    /// in <paramref name="resources"/> are removed.
    /// If <paramref name="removeOthers"/> is <c>false</c> (default), existing resources not
    /// present in <paramref name="resources"/> are left intact.
    /// </summary>
    public void SetResources(IReadOnlyList<Resource> resources, bool removeOthers = false)
    {
        if (removeOthers)
        {
            PeFile.UpdateResources(
                FilePath,
                resources.ToDictionary(r => r.Identifier, r => r.Data)
            );
        }
        else
        {
            var existing = GetResources().ToDictionary(r => r.Identifier, r => r.Data);
            foreach (var resource in resources)
                existing[resource.Identifier] = resource.Data;
            PeFile.UpdateResources(FilePath, existing);
        }
    }

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(Resource resource) => SetResources([resource]);

    /// <summary>
    /// Removes all resources matching the specified predicate.
    /// </summary>
    public void RemoveResources(Func<ResourceIdentifier, bool> predicate)
    {
        var remaining = GetResources().Where(r => !predicate(r.Identifier)).ToArray();
        PeFile.UpdateResources(FilePath, remaining.ToDictionary(r => r.Identifier, r => r.Data));
    }

    /// <summary>
    /// Removes the specified resources.
    /// </summary>
    public void RemoveResources(IReadOnlyList<ResourceIdentifier> identifiers)
    {
        var identifierSet = identifiers.ToHashSet();
        RemoveResources(id => identifierSet.Contains(id));
    }

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void RemoveResources() =>
        PeFile.UpdateResources(FilePath, new Dictionary<ResourceIdentifier, byte[]>());

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);
}

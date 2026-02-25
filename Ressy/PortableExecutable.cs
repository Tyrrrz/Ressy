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
        PeFile.ReadResources(FilePath).Select(r => r.Identifier).ToArray();

    /// <summary>
    /// Gets all existing resources.
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
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(Resource resource) =>
        SetResources(
            GetResources().Where(r => !r.Identifier.Equals(resource.Identifier)).Append(resource)
        );

    /// <summary>
    /// Replaces all existing resources with exactly the specified set.
    /// </summary>
    public void SetResources(IEnumerable<Resource> resources) =>
        PeFile.UpdateResources(FilePath, resources.ToDictionary(r => r.Identifier, r => r.Data));

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);

    /// <summary>
    /// Removes the specified resources.
    /// </summary>
    public void RemoveResources(IEnumerable<ResourceIdentifier> identifiers)
    {
        var identifierSet = identifiers.ToHashSet();
        SetResources(GetResources().Where(r => !identifierSet.Contains(r.Identifier)));
    }

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void RemoveResources() =>
        PeFile.UpdateResources(FilePath, new Dictionary<ResourceIdentifier, byte[]>());
}

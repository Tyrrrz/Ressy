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
        PeFile.ReadResources(FilePath).Select(r => r.Id).ToArray();

    /// <summary>
    /// Gets all existing resources as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<ResourceIdentifier, byte[]> GetResources() =>
        PeFile.ReadResources(FilePath).ToDictionary(r => r.Id, r => r.Data);

    /// <summary>
    /// Gets the raw binary data of the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        var data = PeFile.TryReadResource(FilePath, identifier);
        return data is not null ? new Resource(identifier, data) : null;
    }

    /// <summary>
    /// Gets the raw binary data of the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void ClearResources() =>
        PeFile.UpdateResources(FilePath, new Dictionary<ResourceIdentifier, byte[]>());

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(ResourceIdentifier identifier, byte[] data)
    {
        var resources = GetResources().ToDictionary(kv => kv.Key, kv => kv.Value);
        resources[identifier] = data;
        PeFile.UpdateResources(FilePath, resources);
    }

    /// <summary>
    /// Adds or overwrites multiple resources at once, replacing all existing resources
    /// with exactly the specified set.
    /// </summary>
    public void SetResources(IReadOnlyDictionary<ResourceIdentifier, byte[]> resources) =>
        PeFile.UpdateResources(FilePath, resources);

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier)
    {
        var resources = GetResources().ToDictionary(kv => kv.Key, kv => kv.Value);
        resources.Remove(identifier);
        PeFile.UpdateResources(FilePath, resources);
    }
}

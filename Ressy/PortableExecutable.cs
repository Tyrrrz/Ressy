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

    internal void UpdateResources(
        Action<ResourceUpdateContext> update,
        bool deleteExistingResources = false
    )
    {
        using var context = ResourceUpdateContext.Create(FilePath, deleteExistingResources);
        update(context);
    }

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void ClearResources() => UpdateResources(_ => { }, true);

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(ResourceIdentifier identifier, byte[] data) =>
        UpdateResources(ctx => ctx.Set(identifier, data));

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) =>
        UpdateResources(ctx => ctx.Remove(identifier));
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ressy.PE;

namespace Ressy;

internal partial class ResourceUpdateContext : IDisposable
{
    private readonly string _filePath;
    private readonly Dictionary<ResourceIdentifier, byte[]> _resources;
    private bool _disposed;

    private ResourceUpdateContext(string filePath, Dictionary<ResourceIdentifier, byte[]> resources)
    {
        _filePath = filePath;
        _resources = resources;
    }

    [ExcludeFromCodeCoverage]
    ~ResourceUpdateContext() => Dispose();

    public void Set(ResourceIdentifier identifier, byte[] data) => _resources[identifier] = data;

    public void Remove(ResourceIdentifier identifier) => _resources.Remove(identifier);

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        // Write the complete desired state directly; deleteExisting=true avoids re-reading the file
        PeFile.UpdateResources(
            _filePath,
            existing =>
            {
                foreach (var kv in _resources)
                    existing[kv.Key] = kv.Value;
            },
            deleteExisting: true
        );

        GC.SuppressFinalize(this);
    }
}

internal partial class ResourceUpdateContext
{
    public static ResourceUpdateContext Create(
        string filePath,
        bool deleteExistingResources = false
    )
    {
        var resources = new Dictionary<ResourceIdentifier, byte[]>();

        if (!deleteExistingResources)
        {
            foreach (var (id, data) in PeFile.ReadResources(filePath))
                resources[id] = data;
        }

        return new ResourceUpdateContext(filePath, resources);
    }
}

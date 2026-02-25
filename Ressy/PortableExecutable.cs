using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
public partial class PortableExecutable : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _disposeStream;
    private PEInfo _info;

    // Can be null if this instance is initialized from a raw stream.
    internal string? FilePath { get; }

    /// <summary>
    /// Wraps a seekable stream as a portable executable.
    /// </summary>
    public PortableExecutable(Stream stream, bool disposeStream = false)
    {
        _stream = stream;
        _disposeStream = disposeStream;
        _info = ParsePEInfo(stream);
    }

    /// <summary>
    /// Opens the portable executable at the specified file path.
    /// </summary>
    public PortableExecutable(string filePath)
        : this(
            File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read),
            disposeStream: true
        )
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        if (_info.ResourceSectionIndex < 0)
            return [];

        var resource = _info.Sections[_info.ResourceSectionIndex];
        if (resource.SizeOfRawData == 0 || resource.PointerToRawData == 0)
            return [];

        if (resource.PointerToRawData > int.MaxValue || resource.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)resource.PointerToRawData;
        var sectionSize = (int)resource.SizeOfRawData;

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        return ReadIdentifiers(reader, sectionBase, sectionSize, 0, null, null).ToList();
    }

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    public IReadOnlyList<Resource> GetResources()
    {
        if (_info.ResourceSectionIndex < 0)
            return [];

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        return ReadResourcesFromSection(reader, _info.Sections[_info.ResourceSectionIndex]);
    }

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        if (_info.ResourceSectionIndex < 0)
            return null;

        var resource = _info.Sections[_info.ResourceSectionIndex];
        if (resource.SizeOfRawData == 0 || resource.PointerToRawData == 0)
            return null;

        if (resource.PointerToRawData > int.MaxValue || resource.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var data = FindResourceData(
            reader,
            (int)resource.PointerToRawData,
            (int)resource.SizeOfRawData,
            resource,
            identifier,
            0
        );

        return data is not null ? new Resource(identifier, data) : null;
    }

    /// <summary>
    /// Gets the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    /// <summary>
    /// Adds or overwrites the specified resources, optionally removing the rest.
    /// </summary>
    public void SetResources(IReadOnlyList<Resource> resources, bool removeOthers = false)
    {
        if (removeOthers)
        {
            UpdateResources(resources);
            return;
        }

        var merged = GetResources().ToDictionary(r => r.Identifier);
        foreach (var resource in resources)
            merged[resource.Identifier] = resource;
        UpdateResources(merged.Values.ToArray());
    }

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(Resource resource) => SetResources([resource]);

    /// <summary>
    /// Removes all resources matching the specified predicate.
    /// </summary>
    public void RemoveResources(Func<ResourceIdentifier, bool> predicate) =>
        UpdateResources(GetResources().Where(r => !predicate(r.Identifier)).ToArray());

    /// <summary>
    /// Removes the specified resources.
    /// </summary>
    public void RemoveResources(IReadOnlyList<ResourceIdentifier> identifiers) =>
        RemoveResources(identifiers.ToHashSet().Contains);

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void RemoveResources() => UpdateResources([]);

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposeStream)
            _stream.Dispose();
    }
}

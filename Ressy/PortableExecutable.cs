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
    /// Wraps a seekable, readable, and writable stream as a portable executable.
    /// </summary>
    /// <param name="stream">A seekable stream positioned at the start of a PE file.</param>
    /// <param name="disposeStream">Whether to dispose the stream when this instance is disposed.</param>
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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposeStream)
            _stream.Dispose();
    }

    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        if (_info.RsrcSectionIndex < 0)
            return [];

        var rsrc = _info.Sections[_info.RsrcSectionIndex];
        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return [];

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        var sectionBase = (int)rsrc.PointerToRawData;
        var sectionSize = (int)rsrc.SizeOfRawData;

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var result = new List<ResourceIdentifier>();
        ReadIdentifiers(reader, sectionBase, sectionSize, 0, null, null, result);
        return result;
    }

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    public IReadOnlyList<Resource> GetResources()
    {
        if (_info.RsrcSectionIndex < 0)
            return [];

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        return ReadResourcesFromSection(reader, _info.Sections[_info.RsrcSectionIndex]);
    }

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        if (_info.RsrcSectionIndex < 0)
            return null;

        var rsrc = _info.Sections[_info.RsrcSectionIndex];
        if (rsrc.SizeOfRawData == 0 || rsrc.PointerToRawData == 0)
            return null;

        if (rsrc.PointerToRawData > int.MaxValue || rsrc.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        using var reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        var data = FindResourceData(
            reader,
            (int)rsrc.PointerToRawData,
            (int)rsrc.SizeOfRawData,
            rsrc,
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
            UpdateResources(resources.ToDictionary(r => r.Identifier, r => r.Data));
        }
        else
        {
            var existing = GetResources().ToDictionary(r => r.Identifier, r => r.Data);
            foreach (var resource in resources)
                existing[resource.Identifier] = resource.Data;
            UpdateResources(existing);
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
        var remaining = GetResources()
            .Where(r => !predicate(r.Identifier))
            .ToDictionary(r => r.Identifier, r => r.Data);
        UpdateResources(remaining);
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
    public void RemoveResources() => UpdateResources(new Dictionary<ResourceIdentifier, byte[]>());

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);
}

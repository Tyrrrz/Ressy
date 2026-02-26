using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
public partial class PortableExecutable(
    Stream stream,
    bool isReadOnly = false,
    bool disposeStream = false
) : IDisposable
{
    private PEInfo _info = ParsePEInfo(stream);

    /// <summary>
    /// Underlying stream of the portable executable.
    /// </summary>
    public Stream Stream { get; } = stream;

    /// <summary>
    /// Whether the portable executable is opened in read-only mode.
    /// </summary>
    public bool IsReadOnly { get; } = isReadOnly;

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

        using var reader = new BinaryReader(Stream, Encoding.UTF8, true);
        return ReadIdentifiers(reader, sectionBase, sectionSize, 0, null, null).ToList();
    }

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    public IReadOnlyList<Resource> GetResources()
    {
        if (_info.ResourceSectionIndex < 0)
            return [];

        using var reader = new BinaryReader(Stream, Encoding.UTF8, true);
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

        using var reader = new BinaryReader(Stream, Encoding.UTF8, true);
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
        if (IsReadOnly)
            throw new InvalidOperationException("Cannot modify resources in a read-only PE file.");

        if (removeOthers)
        {
            UpdateResources(resources);
            return;
        }

        var resourcesByIdentifier = GetResources().ToDictionary(r => r.Identifier);

        foreach (var resource in resources)
            resourcesByIdentifier[resource.Identifier] = resource;

        UpdateResources(resourcesByIdentifier.Values.ToArray());
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
        var resourcesToKeep = GetResources().Where(r => !predicate(r.Identifier)).ToArray();
        SetResources(resourcesToKeep, true);
    }

    /// <summary>
    /// Removes the specified resources.
    /// </summary>
    public void RemoveResources(IReadOnlyList<ResourceIdentifier> identifiers) =>
        RemoveResources(identifiers.ToHashSet().Contains);

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    public void RemoveResources() => SetResources([], true);

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);

    /// <summary>
    /// Flushes the underlying stream, ensuring that all changes are written to the PE file.
    /// </summary>
    public void Flush() => Stream.Flush();

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposeStream)
            Stream.Dispose();
    }
}

public partial class PortableExecutable
{
    /// <summary>
    /// Opens the portable executable at the specified file path with the specified access and sharing options.
    /// </summary>
    public static PortableExecutable Open(
        string filePath,
        FileAccess fileAccess,
        FileShare fileShare
    ) =>
        new(
            File.Open(filePath, FileMode.Open, fileAccess, fileShare),
            fileAccess == FileAccess.Read,
            true
        );

    /// <summary>
    /// Opens the portable executable at the specified file path with read and write access.
    /// </summary>
    public static PortableExecutable OpenWrite(string filePath) =>
        Open(filePath, FileAccess.ReadWrite, FileShare.ReadWrite);

    /// <summary>
    /// Opens the portable executable at the specified file path with read-only access.
    /// </summary>
    /// <remarks>
    /// Opening a PE file with read-only access allows reading data when it's in use by another process,
    /// but prevents any modifications to it.
    /// </remarks>
    public static PortableExecutable OpenRead(string filePath) =>
        Open(filePath, FileAccess.Read, FileShare.Read);
}

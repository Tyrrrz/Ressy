using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ressy.Utils.Extensions;

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
/// <remarks>
/// Each satellite stream is wrapped in a read-only <see cref="PortableExecutable" /> instance.
/// When querying resources, satellite resources are merged with the main file's resources,
/// giving preference to the satellite version when identifiers fully match.
/// </remarks>
public partial class PortableExecutable(
    Stream stream,
    IReadOnlyList<Stream> satelliteImageStreams,
    bool disposeStream = false
) : IPortableExecutable
{
    private PEInfo _info = ParsePEInfo(stream);

    private IReadOnlyList<PortableExecutable> _satellites = satelliteImageStreams
        .Select(s => new PortableExecutable(s, [], disposeStream))
        .ToArray();

    // Reads resource identifiers from this file only (excludes satellites).
    private IReadOnlyList<ResourceIdentifier> GetOwnResourceIdentifiers()
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

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        return ReadIdentifiers(reader, sectionBase, sectionSize, 0, null, null).ToList();
    }

    // Reads all resources from this file only (excludes satellites).
    private IReadOnlyList<Resource> GetOwnResources()
    {
        if (_info.ResourceSectionIndex < 0)
            return [];

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        return ReadResourcesFromSection(reader, _info.Sections[_info.ResourceSectionIndex]);
    }

    // Gets a specific resource from this file only (excludes satellites).
    private Resource? TryGetOwnResource(ResourceIdentifier identifier)
    {
        if (_info.ResourceSectionIndex < 0)
            return null;

        var resource = _info.Sections[_info.ResourceSectionIndex];
        if (resource.SizeOfRawData == 0 || resource.PointerToRawData == 0)
            return null;

        if (resource.PointerToRawData > int.MaxValue || resource.SizeOfRawData > int.MaxValue)
            throw new InvalidDataException("Resource section is too large to be processed.");

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
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

    /// <inheritdoc />
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        var own = GetOwnResourceIdentifiers();
        if (_satellites.Count == 0)
            return own;

        var seen = new HashSet<ResourceIdentifier>(own);

        foreach (var satellite in _satellites)
        {
            foreach (var id in satellite.GetResourceIdentifiers())
            {
                seen.Add(id);
            }
        }

        return seen.ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<Resource> GetResources()
    {
        var own = GetOwnResources();
        if (_satellites.Count == 0)
            return own;

        var dict = own.ToDictionary(r => r.Identifier);

        foreach (var satellite in _satellites)
        foreach (var r in satellite.GetResources())
            dict[r.Identifier] = r;

        return dict.Values.ToArray();
    }

    /// <inheritdoc />
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        // Check satellites first (they take priority)
        foreach (var satellite in _satellites)
        {
            var resource = satellite.TryGetResource(identifier);
            if (resource is not null)
                return resource;
        }

        return TryGetOwnResource(identifier);
    }

    /// <inheritdoc />
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    /// <inheritdoc />
    public void SetResources(IReadOnlyList<Resource> resources, bool removeOthers = false)
    {
        if (removeOthers)
        {
            UpdateResources(resources);
            return;
        }

        var resourcesByIdentifier = GetOwnResources().ToDictionary(r => r.Identifier);

        foreach (var resource in resources)
            resourcesByIdentifier[resource.Identifier] = resource;

        UpdateResources(resourcesByIdentifier.Values.ToArray());
    }

    /// <inheritdoc />
    public void SetResource(Resource resource) => SetResources([resource]);

    /// <inheritdoc />
    public void RemoveResources(Func<ResourceIdentifier, bool> predicate)
    {
        var resourcesToKeep = GetOwnResources().Where(r => !predicate(r.Identifier)).ToArray();
        SetResources(resourcesToKeep, true);
    }

    /// <inheritdoc />
    public void RemoveResources(IReadOnlyList<ResourceIdentifier> identifiers) =>
        RemoveResources(identifiers.ToHashSet().Contains);

    /// <inheritdoc />
    public void RemoveResources() => SetResources([], true);

    /// <inheritdoc />
    public void RemoveResource(ResourceIdentifier identifier) => RemoveResources([identifier]);

    /// <inheritdoc />
    public void Dispose()
    {
        _satellites.DisposeAll();

        stream.Flush();

        if (disposeStream)
            stream.Dispose();
    }
}

public partial class PortableExecutable
{
    private static readonly Regex LocalePattern = new(
        @"^[a-zA-Z]{2,3}(-[a-zA-Z0-9]{1,8})*$",
        RegexOptions.Compiled
    );

    private static IReadOnlyList<Stream> DiscoverSatelliteStreams(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            return [];

        var muiFileName = Path.GetFileName(filePath) + ".mui";
        var streams = new List<Stream>();

        try
        {
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var localeName = Path.GetFileName(subDir);
                if (localeName is null || !LocalePattern.IsMatch(localeName))
                    continue;

                var muiPath = Path.Combine(subDir, muiFileName);
                if (!File.Exists(muiPath))
                    continue;

                try
                {
                    streams.Add(File.Open(muiPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                }
                catch (IOException)
                {
                    // Skip inaccessible satellite files
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip inaccessible satellite files
                }
            }
        }
        catch
        {
            // Dispose any already-opened streams on unexpected failure
            streams.DisposeAll();
            throw;
        }

        return streams;
    }

    /// <summary>
    /// Opens the portable executable at the specified file path with the specified access and sharing options.
    /// </summary>
    public static PortableExecutable Open(
        string filePath,
        FileAccess fileAccess,
        FileShare fileShare,
        bool openSatellites = false
    )
    {
        var mainStream = File.Open(filePath, FileMode.Open, fileAccess, fileShare);

        if (!openSatellites)
            return new(mainStream, [], true);

        var satelliteStreams = DiscoverSatelliteStreams(filePath);
        return new(mainStream, satelliteStreams, true);
    }

    /// <summary>
    /// Opens the portable executable with read/write access from the specified stream.
    /// </summary>
    /// <remarks>
    /// Satellite file discovery is not available for stream-based access.
    /// When initializing from a stream, make sure that the stream supports seeking.
    /// </remarks>
    public static IPortableExecutable OpenWrite(Stream stream) =>
        new PortableExecutable(stream, []);

    /// <summary>
    /// Opens the portable executable at the specified file path with read/write access.
    /// </summary>
    /// <remarks>
    /// Write operations only affect the main PE file.
    /// If <paramref name="openSatellites" /> is <see langword="true" />, satellite files are used
    /// exclusively for reading — to modify a satellite file's resources, open it directly.
    /// </remarks>
    public static IPortableExecutable OpenWrite(string filePath, bool openSatellites = false) =>
        Open(filePath, FileAccess.ReadWrite, FileShare.None, openSatellites);

    /// <summary>
    /// Opens the portable executable with read-only access from the specified stream.
    /// </summary>
    /// <remarks>
    /// Satellite file discovery is not available for stream-based access.
    /// When initializing from a stream, make sure that the stream supports seeking.
    /// </remarks>
    public static IReadOnlyPortableExecutable OpenRead(Stream stream) =>
        new PortableExecutable(stream, []);

    /// <summary>
    /// Opens the portable executable at the specified file path with read-only access.
    /// </summary>
    /// <remarks>
    /// Opening a PE file with read-only access allows reading data when the file is in use by another process
    /// (e.g. to extract resources from a currently running executable), but prevents any modifications to it.
    /// </remarks>
    public static IReadOnlyPortableExecutable OpenRead(
        string filePath,
        bool openSatellites = false
    ) => Open(filePath, FileAccess.Read, FileShare.Read, openSatellites);
}

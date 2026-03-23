using System;
using System.Collections.Generic;
using System.Globalization;
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
    private IReadOnlyList<PortableExecutable> _satellites = [];

    /// <summary>
    /// Initializes a new instance of <see cref="PortableExecutable" /> with satellite resource streams.
    /// </summary>
    /// <remarks>
    /// Each satellite stream is wrapped in a read-only <see cref="PortableExecutable" /> instance.
    /// When querying resources, satellite resources are merged with the main file's resources,
    /// giving preference to the satellite version when identifiers fully match.
    /// </remarks>
    public PortableExecutable(
        Stream neutralImageStream,
        IReadOnlyList<Stream> satelliteImageStreams,
        bool isReadOnly = false,
        bool disposeStream = false
    )
        : this(neutralImageStream, isReadOnly, disposeStream)
    {
        _satellites = satelliteImageStreams
            .Select(s => new PortableExecutable(s, true, disposeStream))
            .ToList();
    }

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

    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, this method returns the union of identifiers
    /// from the main file and all satellite files.
    /// </remarks>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        var own = GetOwnResourceIdentifiers();
        if (_satellites.Count == 0)
            return own;

        var seen = new HashSet<ResourceIdentifier>(own);
        var result = new List<ResourceIdentifier>(own);

        foreach (var satellite in _satellites)
        foreach (var id in satellite.GetResourceIdentifiers())
            if (seen.Add(id))
                result.Add(id);

        return result;
    }

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, resources from satellite files override
    /// the main file's resources when their identifiers fully match.
    /// </remarks>
    public IReadOnlyList<Resource> GetResources()
    {
        var own = GetOwnResources();
        if (_satellites.Count == 0)
            return own;

        var dict = own.ToDictionary(r => r.Identifier);

        foreach (var satellite in _satellites)
        foreach (var r in satellite.GetResources())
            dict[r.Identifier] = r;

        return dict.Values.ToList();
    }

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, the satellite version takes preference
    /// when the identifier fully matches.
    /// </remarks>
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

    /// <summary>
    /// Gets the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier)
        ?? throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    /// <summary>
    /// Adds or overwrites the specified resources, optionally removing the rest.
    /// </summary>
    /// <remarks>
    /// Write operations only affect the main file, not satellite files.
    /// </remarks>
    public void SetResources(IReadOnlyList<Resource> resources, bool removeOthers = false)
    {
        if (isReadOnly)
            throw new InvalidOperationException("Cannot modify resources in a read-only PE file.");

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

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    public void SetResource(Resource resource) => SetResources([resource]);

    /// <summary>
    /// Removes all resources matching the specified predicate.
    /// </summary>
    /// <remarks>
    /// Write operations only affect the main file, not satellite files.
    /// </remarks>
    public void RemoveResources(Func<ResourceIdentifier, bool> predicate)
    {
        var resourcesToKeep = GetOwnResources().Where(r => !predicate(r.Identifier)).ToArray();
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

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var satellite in _satellites)
            satellite.Dispose();

        stream.Flush();

        if (disposeStream)
            stream.Dispose();
    }
}

public partial class PortableExecutable
{
    private static bool IsValidLocale(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        try
        {
            var culture = CultureInfo.GetCultureInfo(name);
            return !string.IsNullOrEmpty(culture.Name);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

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
                if (localeName is null || !IsValidLocale(localeName))
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
            foreach (var s in streams)
                s.Dispose();
            throw;
        }

        return streams;
    }

    /// <summary>
    /// Creates a <see cref="PortableExecutable" /> from a stream.
    /// </summary>
    public static PortableExecutable FromStream(
        Stream stream,
        bool isReadOnly = false,
        bool disposeStream = false
    ) => new(stream, isReadOnly, disposeStream);

    /// <summary>
    /// Creates a <see cref="PortableExecutable" /> from a file stream,
    /// optionally discovering and including satellite MUI resource files.
    /// </summary>
    /// <remarks>
    /// Satellite files are located by searching for <c>&lt;filename&gt;.mui</c>
    /// files in immediate subdirectories whose names are valid locale identifiers
    /// (e.g. <c>en-US/app.exe.mui</c>).
    /// </remarks>
    public static PortableExecutable FromStream(
        FileStream stream,
        bool openSatellites,
        bool disposeStream = false
    )
    {
        var readOnly = !stream.CanWrite;

        if (!openSatellites)
            return new(stream, readOnly, disposeStream);

        var satelliteStreams = DiscoverSatelliteStreams(stream.Name);
        return new(stream, satelliteStreams, readOnly, disposeStream);
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
        var readOnly = fileAccess == FileAccess.Read;

        if (!openSatellites)
            return new(mainStream, readOnly, true);

        var satelliteStreams = DiscoverSatelliteStreams(filePath);
        return new(mainStream, satelliteStreams, readOnly, true);
    }

    /// <summary>
    /// Opens the portable executable at the specified file path with read and write access.
    /// </summary>
    public static PortableExecutable OpenWrite(string filePath) =>
        Open(filePath, FileAccess.ReadWrite, FileShare.None);

    /// <summary>
    /// Opens the portable executable at the specified file path with read-only access.
    /// </summary>
    /// <remarks>
    /// Opening a PE file with read-only access allows reading data when the file is in use by another process
    /// (e.g. to extract resources from a currently running executable), but prevents any modifications to it.
    /// </remarks>
    public static PortableExecutable OpenRead(string filePath, bool openSatellites = false) =>
        Open(filePath, FileAccess.Read, FileShare.Read, openSatellites);
}

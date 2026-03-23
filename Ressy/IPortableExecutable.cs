using System;
using System.Collections.Generic;

namespace Ressy;

/// <summary>
/// Read/write access to a portable executable image file.
/// </summary>
/// <remarks>
/// Write operations only affect the main PE file.
/// If satellite files are included, they are used exclusively for reading.
/// To modify a satellite file's resources, open it directly as a <see cref="PortableExecutable" />.
/// </remarks>
public interface IPortableExecutable : IReadOnlyPortableExecutable
{
    /// <summary>
    /// Adds or overwrites the specified resources, optionally removing the rest.
    /// </summary>
    /// <remarks>
    /// Write operations only affect the main file, not satellite files.
    /// </remarks>
    void SetResources(IReadOnlyList<Resource> resources, bool removeOthers = false);

    /// <summary>
    /// Adds or overwrites the specified resource.
    /// </summary>
    void SetResource(Resource resource);

    /// <summary>
    /// Removes all resources matching the specified predicate.
    /// </summary>
    /// <remarks>
    /// Write operations only affect the main file, not satellite files.
    /// </remarks>
    void RemoveResources(Func<ResourceIdentifier, bool> predicate);

    /// <summary>
    /// Removes the specified resources.
    /// </summary>
    void RemoveResources(IReadOnlyList<ResourceIdentifier> identifiers);

    /// <summary>
    /// Removes all existing resources.
    /// </summary>
    void RemoveResources();

    /// <summary>
    /// Removes the specified resource.
    /// </summary>
    void RemoveResource(ResourceIdentifier identifier);
}

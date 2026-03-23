using System;
using System.Collections.Generic;

namespace Ressy;

/// <summary>
/// Read-only access to a portable executable image file.
/// </summary>
public interface IReadOnlyPortableExecutable : IDisposable
{
    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, this method returns the union of identifiers
    /// from the main file and all satellite files.
    /// </remarks>
    IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers();

    /// <summary>
    /// Gets all existing resources, along with their stored binary data.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, resources from satellite files override
    /// the main file's resources when their identifiers fully match.
    /// </remarks>
    IReadOnlyList<Resource> GetResources();

    /// <summary>
    /// Gets the specified resource.
    /// Returns <c>null</c> if the resource doesn't exist.
    /// </summary>
    /// <remarks>
    /// If satellite files are included, the satellite version takes preference
    /// when the identifier fully matches.
    /// </remarks>
    Resource? TryGetResource(ResourceIdentifier identifier);

    /// <summary>
    /// Gets the specified resource.
    /// </summary>
    Resource GetResource(ResourceIdentifier identifier);
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Ressy.Native;

// ReSharper disable AccessToDisposedClosure

namespace Ressy;

/// <summary>
/// Portable executable image file.
/// </summary>
public class PortableExecutable
{
    /// <summary>
    /// Path to the portable executable image file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes an instance of <see cref="PortableExecutable" />.
    /// </summary>
    public PortableExecutable(string filePath) => FilePath = filePath;

    /// <summary>
    /// Gets the identifiers of all existing resources.
    /// </summary>
    public IReadOnlyList<ResourceIdentifier> GetResourceIdentifiers()
    {
        using var library = NativeLibrary.LoadAsDataFile(FilePath);

        IReadOnlyList<ResourceType> GetResourceTypes()
        {
            var result = new List<ResourceType>();

            NativeHelpers.ThrowIfError(() =>
                NativeMethods.EnumResourceTypesEx(
                    library.Handle,
                    (_, typeHandle, _) =>
                    {
                        result.Add(ResourceType.FromHandle(typeHandle));
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return result;
        }

        IReadOnlyList<ResourceName> GetResourceNames(ResourceType type)
        {
            using var typeMarshaled = type.Marshal();

            var result = new List<ResourceName>();

            NativeHelpers.ThrowIfError(() =>
                NativeMethods.EnumResourceNamesEx(
                    library.Handle, typeMarshaled.Handle,
                    (_, _, nameHandle, _) =>
                    {
                        result.Add(ResourceName.FromHandle(nameHandle));
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return result;
        }

        IReadOnlyList<Language> GetResourceLanguages(ResourceType type, ResourceName name)
        {
            using var typeMarshaled = type.Marshal();
            using var nameMarshaled = name.Marshal();

            var result = new List<Language>();

            NativeHelpers.ThrowIfError(() =>
                NativeMethods.EnumResourceLanguagesEx(
                    library.Handle, typeMarshaled.Handle, nameMarshaled.Handle,
                    (_, _, _, languageId, _) =>
                    {
                        result.Add(new Language(languageId));
                        return true;
                    },
                    IntPtr.Zero, 0, 0
                )
            );

            return result;
        }

        return (
            from type in GetResourceTypes()
            from name in GetResourceNames(type)
            from language in GetResourceLanguages(type, name)
            select new ResourceIdentifier(type, name, language)
        ).ToArray();
    }

    /// <summary>
    /// Gets the raw binary data of the specified resource.
    /// Returns <c>null</c> if the resource does not exist.
    /// </summary>
    public Resource? TryGetResource(ResourceIdentifier identifier)
    {
        using var library = NativeLibrary.LoadAsDataFile(FilePath);
        using var typeMarshaled = identifier.Type.Marshal();
        using var nameMarshaled = identifier.Name.Marshal();

        var resourceHandle = NativeMethods.FindResourceEx(
            library.Handle,
            typeMarshaled.Handle,
            nameMarshaled.Handle,
            (ushort)identifier.Language.Id
        );

        if (resourceHandle == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();

            // Return null if the resource does not exist
            if (errorCode is 1813 or 1814 or 1815)
                return null;

            // Throw in other cases
            throw new Win32Exception(errorCode);
        }

        var dataHandle = NativeHelpers.ThrowIfError(() =>
            NativeMethods.LoadResource(library.Handle, resourceHandle)
        );

        var dataSource = NativeHelpers.ThrowIfError(() =>
            NativeMethods.LockResource(dataHandle)
        );

        var length = NativeHelpers.ThrowIfError(() =>
            NativeMethods.SizeofResource(library.Handle, resourceHandle)
        );

        var data = new byte[length];
        Marshal.Copy(dataSource, data, 0, (int)length);

        return new Resource(identifier, data);
    }

    /// <summary>
    /// Gets the raw binary data of the specified resource.
    /// </summary>
    public Resource GetResource(ResourceIdentifier identifier) =>
        TryGetResource(identifier) ??
        throw new InvalidOperationException($"Resource '{identifier}' does not exist.");

    internal void UpdateResources(Action<ResourceUpdateContext> update, bool deleteExistingResources = false)
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
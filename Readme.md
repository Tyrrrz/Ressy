# Ressy

[![Build](https://github.com/Tyrrrz/Ressy/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/Ressy/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/Ressy/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/Ressy)
[![Version](https://img.shields.io/nuget/v/Ressy.svg)](https://nuget.org/packages/Ressy)
[![Downloads](https://img.shields.io/nuget/dt/Ressy.svg)](https://nuget.org/packages/Ressy)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

✅ **Project status: active**.

Ressy is a library for reading and writing native resources stored in portable executable images (i.e. EXE and DLL files).
It offers a high-level abstraction model for working with [resource functions](https://docs.microsoft.com/en-us/windows/win32/menurc/resources-functions) provided by the Windows API.

> ⚠️ This library relies on Windows API and, as such, works only on Windows.

💬 **If you want to chat, join my [Discord server](https://discord.gg/2SUWKFnHSm)**.

## Download

📦 [NuGet](https://nuget.org/packages/Ressy): `dotnet add package Ressy`

## Usage

Ressy's functionality is provided entirely through the `PortableExecutable` class.
You can create an instance of this class by passing a string that specifies the path to a PE file:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

// ...
```

### Enumerating and reading resources

To get the list of resources in a PE file, use the `GetResourceIdentifiers()` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var identifiers = portableExecutable.GetResourceIdentifiers();

// Returned list contains:
// - { Type: 16 (RT_VERSION), Name: 1, Language: 1033 }
// - { Type: 24 (RT_MANIFEST), Name: 1, Language: 1033 }
// - { Type: 3 (RT_ICON), Name: 1, Language: 1033 }
// - { Type: 3 (RT_ICON), Name: 2, Language: 1033 }
// - { Type: 3 (RT_ICON), Name: 3, Language: 1033 }
// - { Type: 14 (RT_GROUP_ICON), Name: 2, Language: 1033 }
// - { Type: 4 (RT_MENU), Name: 1, Language: 1033 }
// - { Type: 5 (RT_DIALOG), Name: 1, Language: 1033 }
// - { Type: 5 (RT_DIALOG), Name: 2, Language: 1033 }
// - { Type: 5 (RT_DIALOG), Name: 3, Language: 1033 }
// - { Type: "EDPENLIGHTENEDAPPINFOID", Name: "MICROSOFTEDPENLIGHTENEDAPPINFO", Language: 1033 }
// - { Type: "EDPPERMISSIVEAPPINFOID", Name: "MICROSOFTEDPPERMISSIVEAPPINFO", Language: 1033 }
// - { Type: "MUI", Name: 1, Language: 1033 }
// ... (truncated) ...
```

To resolve a specific resource, you can call the `GetResource(...)` method.
This returns an instance of the `Resource` class, which contains the resource data:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var resource = portableExecutable.GetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(1),
    new ResourceLanguage(1033)
));

var resourceData = resource.Data; // byte[]
var resourceString = resource.ReadAsString(Encoding.UTF8); // string
```

If you aren't sure if the requested resource exists in the PE file, you can also use the `TryGetResource(...)` method instead.
It returns `null` in case the resource is missing:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var resource = portableExecutable.TryGetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(100),
    new ResourceLanguage(1033)
)); // resource is null
```

### Modifying resources

To add or overwrite a resource, call the `SetResource(...)` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new ResourceLanguage(1033)
    ),
    new byte[] { 0x01, 0x02, 0x03 }
);
```

To remove a resource, call the `RemoveResource(...)` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new ResourceLanguage(1033)
    )
);
```

To remove all resources in a PE file, call the `ClearResources()` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.ClearResources();
```

### High-level operations

Ressy provides extensions for `PortableExecutable` that enable you to directly read and manipulate known resources types, such as icons, manifests, versions, etc.

#### Manifest resources

To read the manifest resource as a text string, call the `GetManifest()` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var manifest = portableExecutable.GetManifest();
// -or-
// var manifest = portableExecutable.TryGetManifest();
```

> 💡 In case of multiple manifest resources, this method retrieves the one with the lowest ordinal resource name in the neutral language.
If there are no resources matching aforementioned criteria, this method retrieves the first manifest resource it encounters.

To remove all manifest resources, call the `RemoveManifest()` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveManifest();
```

To add or overwrite a manifest resource, call the `SetManifest(...)` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetManifest("<assembly>...</assembly>"); 
```

#### Icon resources

To remove all icon and icon group resources, call the `RemoveIcon()` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveIcon();
```

To add or overwrite icon resources based on an ICO file, call the `SetIcon(...)` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetIcon("new_icon.ico");
```

> ⚠️ Calling this method does not remove existing icon resources, except for those that are overwritten directly.
If you want to clean out redundant icon resources (e.g. if the previous icon group contained more icons), call the `RemoveIcon()` method first.

Additionally, you can also set the icon by passing a stream that contains ICO-formatted data:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

using var iconFileStream = File.Open("new_icon.ico");
portableExecutable.SetIcon(iconFileStream);
```

#### Version info resources

To get the version info resource, call the `GetVersionInfo()` extension method.
This returns a `VersionInfo` object that represents the deserialized binary data stored in the resource:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var versionInfo = portableExecutable.GetVersionInfo();
// -or-
// var versionInfo = portableExecutable.TryGetVersionInfo();

// Returned object contains:
// {
//   FileVersion: 10.0.19041.1
//   ProductVersion: 10.0.19041.1
//   FileFlags: FileFlags.None
//   FileOperatingSystem: FileOperatingSystem.Windows32
//   FileType: FileType.App
//   FileSubType: FileSubType.Unknown
//   Attributes: [
//     VersionAttributeName.FileVersion: "10.0.19041.1 (WinBuild.160101.0800)"
//     VersionAttributeName.ProductVersion: "10.0.19041.1"
//     VersionAttributeName.ProductName: "Microsoft® Windows® Operating System"
//     VersionAttributeName.FileDescription: "Notepad"
//     VersionAttributeName.CompanyName: "Microsoft Corporation"
//     ...
//   ]
//   Translations: [
//     { LanguageId: 0, Codepage: 1200 }
//   ]
// }
```

> 💡 In case of multiple version info resources, this method retrieves the one with the lowest ordinal resource name in the neutral language.
If there are no resources matching aforementioned criteria, this method retrieves the first version info resource it encounters.

To remove all version info resources, call the `RemoveVersionInfo()` extension method:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveVersionInfo();
```

To add or overwrite a version info resource, call the `SetVersionInfo(...)` extension method.
You can use the `VersionInfoBuilder` class to simplify the creation of a new `VersionInfo` instance:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var versionInfo = new VersionInfoBuilder()
    .SetFileVersion(new Version(1, 2, 3, 4))
    .SetProductVersion(new Version(1, 2, 3, 4))
    .SetFileType(FileType.App)
    .SetAttribute(VersionAttributeName.FileDescription, "My new description")
    .SetAttribute(VersionAttributeName.CompanyName, "My new company")
    .SetAttribute("Custom Attribute", "My new value")
    .Build();

portableExecutable.SetVersionInfo(versionInfo);
```

Alternatively, you can also use this method to modify specific properties in a currently stored version info resource, leaving the rest intact:

```csharp
using Ressy;
using Ressy.Abstractions;

var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetVersionInfo(v => v
    .SetFileVersion(new Version(1, 2, 3, 4))
    .SetAttribute("Custom Attribute", "My new value")
);
```

> 💡 When using this overload of `SetVersionInfo(...)`, properties that were not provided are taken from the existing version info resource.
If there is no version info resource in the PE file, this method will resort to default values instead.
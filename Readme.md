# Ressy

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://tyrrrz.me/ukraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/Ressy/main.yml?branch=master)](https://github.com/Tyrrrz/Ressy/actions)
[![Coverage](https://img.shields.io/codecov/c/github/Tyrrrz/Ressy/master)](https://codecov.io/gh/Tyrrrz/Ressy)
[![Version](https://img.shields.io/nuget/v/Ressy.svg)](https://nuget.org/packages/Ressy)
[![Downloads](https://img.shields.io/nuget/dt/Ressy.svg)](https://nuget.org/packages/Ressy)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**Ressy** is a library for managing native resources stored in portable executable images (i.e. EXE and DLL files).
It offers a high-level abstraction model for working with the [resource functions](https://docs.microsoft.com/en-us/windows/win32/menurc/resources-functions) provided by the Windows API.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me/ukraine). Glory to Ukraine! 🇺🇦

## Install

- 📦 [NuGet](https://nuget.org/packages/Ressy): `dotnet add package Ressy`

> **Warning**:
> This library relies on the Windows API and, as such, works only on Windows.

## Usage

**Ressy**'s functionality is provided entirely through the `PortableExecutable` class.
You can create an instance of this class by passing a string that specifies the path to a PE file:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

// ...
```

### Reading resources

#### Enumerate resource identifiers

To get the list of resources in a PE file, use the `GetResourceIdentifiers()` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

var identifiers = portableExecutable.GetResourceIdentifiers();
```

Returned list should contain something similiar to this:

```txt
- Type: 16 (RT_VERSION), Name: 1, Language: 1033
- Type: 24 (RT_MANIFEST), Name: 1, Language: 1033
- Type: 3 (RT_ICON), Name: 1, Language: 1033
- Type: 3 (RT_ICON), Name: 2, Language: 1033
- Type: 3 (RT_ICON), Name: 3, Language: 1033
- Type: 14 (RT_GROUP_ICON), Name: 2, Language: 1033
- Type: 4 (RT_MENU), Name: 1, Language: 1033
- Type: 5 (RT_DIALOG), Name: 1, Language: 1033
- Type: 5 (RT_DIALOG), Name: 2, Language: 1033
- Type: 5 (RT_DIALOG), Name: 3, Language: 1033
- Type: "MUI", Name: 1, Language: 1033
- ...
```

#### Retrieve resource data

To resolve a specific resource, call the `GetResource(...)` method.
This returns an instance of the `Resource` class that contains the resource data:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

var resource = portableExecutable.GetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(1),
    new Language(1033)
));

var resourceData = resource.Data; // byte[]
var resourceString = resource.ReadAsString(Encoding.UTF8); // string
```

If you aren't sure that the requested resource actually exists in the PE file, you can use the `TryGetResource(...)` method instead.
It returns `null` in case the resource is missing:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

var resource = portableExecutable.TryGetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(100),
    new Language(1033)
)); // resource is null
```

### Modifying resources

#### Set resource data

To add or overwrite a resource, call the `SetResource(...)` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.SetResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new Language(1033)
    ),
    [0x01, 0x02, 0x03]
);
```

#### Remove resources

To remove a resource, call the `RemoveResource(...)` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.RemoveResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new Language(1033)
    )
);
```

To remove all resources in a PE file, call the `ClearResources()` method:

```csharp
using Ressy;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.ClearResources();
```

### High-level operations

**Ressy** provides extensions for `PortableExecutable` that enable you to directly read and manipulate known resource types, such as icons, manifests, versions, etc.

#### Manifest resources

A manifest resource (type `24`) contains XML data that identifies and describes native assemblies that the application should bind to at run-time.
It may also contain other information, such as application settings, requested execution level, and more.

To learn more about application manifests, see [this article](https://docs.microsoft.com/en-us/windows/win32/sbscs/application-manifests).

##### Retrieve the manifest

To read the manifest resource as an XML text string, call the `GetManifest()` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Manifests;

var portableExecutable = new PortableExecutable("some_app.exe");

var manifest = portableExecutable.GetManifest();
// -or-
// var manifest = portableExecutable.TryGetManifest();
```

> **Note**:
> If there are multiple manifest resources, this method retrieves the first one it finds, giving preference to resources with lower ordinal name (ID) and in the neutral language.

##### Set the manifest

To add or overwrite a manifest resource, call the `SetManifest(...)` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Manifests;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.SetManifest("<assembly>...</assembly>");
```

##### Remove the manifest

To remove all manifest resources, call the `RemoveManifest()` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Manifests;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.RemoveManifest();
```

#### Icon resources

Icon resources (type `3`) and icon group resources (type `14`) are used to visually identify an application within the operating system.
Each portable executable file may contain multiple icon resources (usually in different sizes or color configurations), which are grouped together by the corresponding icon group resource.

##### Set the icon

To add or overwrite icon resources based on an ICO file, call the `SetIcon(...)` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Icons;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.SetIcon("new_icon.ico");
```

> **Warning**:
> Calling this method does not remove the existing icon and icon group resources, except for those that are overwritten directly.
> If you want to clean out redundant icon resources, call the `RemoveIcon()` method first.

Additionally, you can also set the icon by passing a stream that contains ICO-formatted data:

```csharp
using Ressy;
using Ressy.HighLevel.Icons;

var portableExecutable = new PortableExecutable("some_app.exe");

using var iconFileStream = File.OpenRead("new_icon.ico");
portableExecutable.SetIcon(iconFileStream);
```

##### Remove the icon

To remove all icon and icon group resources, call the `RemoveIcon()` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Icons;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.RemoveIcon();
```

#### Version info resources

A version info resource (type `16`) contains file version numbers, compatibility flags, and arbitrary string attributes.
Some of these attributes (such as, for example, `ProductName` and `Copyright`) are recognized by the operating system and may be displayed in certain places.

##### Retrieve version info

To get the version info resource, call the `GetVersionInfo()` extension method.
This returns a `VersionInfo` object that represents the deserialized binary data stored in the resource:

```csharp
using Ressy;
using Ressy.HighLevel.Versions;

var portableExecutable = new PortableExecutable("some_app.exe");

var versionInfo = portableExecutable.GetVersionInfo();
// -or-
// var versionInfo = portableExecutable.TryGetVersionInfo();
```

Returned object should contain data similar to this:

```jsonc
// Formatted as JSON in this example for better readability
{
  "FileVersion": "10.0.19041.1",
  "ProductVersion": "10.0.19041.1",
  "FileFlags": "None",
  "FileOperatingSystem": "Windows32, WindowsNT",
  "FileType": "Application",
  "FileSubType": "Unknown",
  "AttributeTables": [
    {
      "Language": {
        "Id": 1033
      },
      "CodePage": {
        "Id": 1200
      },
      "Attributes": {
        "CompanyName": "Microsoft Corporation",
        "FileDescription": "Notepad",
        "FileVersion": "10.0.19041.1 (WinBuild.160101.0800)",
        "InternalName": "Notepad",
        "LegalCopyright": "© Microsoft Corporation. All rights reserved.",
        "OriginalFilename": "NOTEPAD.EXE.MUI",
        "ProductName": "Microsoft® Windows® Operating System",
        "ProductVersion": "10.0.19041.1"
      }
    }
  ]
}
```

> **Note**:
> If there are multiple version info resources, this method retrieves the first one it finds, giving preference to resources with lower ordinal name (ID) and in the neutral language.

When working with version info resources that include multiple attribute tables (bound to different language and code page pairs), you can use the `GetAttribute(...)` method to query a specific attribute.
This method searches through all attribute tables (giving preference to tables in the neutral language) and returns the first matching value it finds:

```csharp
// ...

var companyName = versionInfo.GetAttribute(VersionAttributeName.CompanyName); // Microsoft Corporation
// -or-
// var companyName = versionInfo.TryGetAttribute(VersionAttributeName.CompanyName);
```

##### Set version info

To add or overwrite a version info resource, call the `SetVersionInfo(...)` extension method.
You can use the `VersionInfoBuilder` class to drastically simplify the creation of a new `VersionInfo` instance:

```csharp
using Ressy;
using Ressy.HighLevel.Versions;

var portableExecutable = new PortableExecutable("some_app.exe");

var versionInfo = new VersionInfoBuilder()
    .SetFileVersion(new Version(1, 2, 3, 4))
    .SetProductVersion(new Version(1, 2, 3, 4))
    .SetFileType(FileType.Application)
    .SetAttribute(VersionAttributeName.FileDescription, "My new description")
    .SetAttribute(VersionAttributeName.CompanyName, "My new company")
    .SetAttribute("Custom Attribute", "My new value")
    .Build();

portableExecutable.SetVersionInfo(versionInfo);
```

You can also use an alternative overload of this method, which lets you selectively modify only a subset of properties in a version info resource, leaving the rest intact.
Properties that are not provided are pulled from the existing version info resource or resolved to their default values in case the resource does not exist:

```csharp
using Ressy;
using Ressy.HighLevel.Versions;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.SetVersionInfo(v => v
    .SetFileVersion(new Version(1, 2, 3, 4))
    .SetAttribute("Custom Attribute", "My new value")
);
```

> **Note**:
> When using the `SetAttribute(...)` method on `VersionInfoBuilder`, you can optionally specify the language and code page of the table that you want to add the attribute to.
> If you choose to omit these parameters, **Ressy** will set the attribute in all attribute tables.
> In case there are no existing attribute tables, this method creates a new one bound to the neutral language and the Unicode code page.

##### Remove version info

To remove all version info resources, call the `RemoveVersionInfo()` extension method:

```csharp
using Ressy;
using Ressy.HighLevel.Versions;

var portableExecutable = new PortableExecutable("some_app.exe");

portableExecutable.RemoveVersionInfo();
```

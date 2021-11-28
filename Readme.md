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

Include the namespace:
```csharp
using Ressy;
```

```csharp
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

// ...
```

### Reading resources

#### Enumerating identifiers

To get the list of resources in a PE file, use the `GetResourceIdentifiers()` method:

<!-- snippet: EnumeratingIdentifiers -->
<a id='snippet-enumeratingidentifiers'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var identifiers = portableExecutable.GetResourceIdentifiers();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L26-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-enumeratingidentifiers' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Returned list should contain:

<!-- snippet: Snippets.EnumeratingIdentifiers.verified.txt -->
<a id='snippet-Snippets.EnumeratingIdentifiers.verified.txt'></a>
```txt
[
  - Type: EDPENLIGHTENEDAPPINFOID 1033, Name: , Language: 1033,
  - Type: EDPPERMISSIVEAPPINFOID 1033, Name: , Language: 1033,
  - Type: MUI 1033, Name: 1, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 1, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 2, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 3, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 4, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 5, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 6, Language: 1033,
  - Type: #3 (ICON) 1033, Name: 7, Language: 1033
]
```
<sup><a href='/Ressy.Tests/Snippets.EnumeratingIdentifiers.verified.txt#L1-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-Snippets.EnumeratingIdentifiers.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Retrieving data

To resolve a specific resource, you can call the `GetResource(...)` method.
This returns an instance of the `Resource` class, which contains the resource data:

<!-- snippet: RetrievingData -->
<a id='snippet-retrievingdata'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var resource = portableExecutable.GetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(1),
    new Language(1033)
));

var resourceData = resource.Data; // byte[]
var resourceString = resource.ReadAsString(Encoding.UTF8); // string
```
<sup><a href='/Ressy.Tests/Snippets.cs#L60-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-retrievingdata' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If you aren't sure if the requested resource exists in the PE file, you can also use the `TryGetResource(...)` method instead.
It returns `null` in case the resource is missing:

<!-- snippet: TryGetResource -->
<a id='snippet-trygetresource'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var resource = portableExecutable.TryGetResource(new ResourceIdentifier(
    ResourceType.Manifest,
    ResourceName.FromCode(100),
    new Language(1033)
)); // resource is null
```
<sup><a href='/Ressy.Tests/Snippets.cs#L42-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-trygetresource' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Modifying resources

#### Setting resource data

To add or overwrite a resource, call the `SetResource(...)` method:

<!-- snippet: SetResource -->
<a id='snippet-setresource'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new Language(1033)
    ),
    new byte[] {0x01, 0x02, 0x03}
);
```
<sup><a href='/Ressy.Tests/Snippets.cs#L85-L98' title='Snippet source file'>snippet source</a> | <a href='#snippet-setresource' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Removing a resource

To remove a resource, call the `RemoveResource(...)` method:

<!-- snippet: SetResource -->
<a id='snippet-setresource'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetResource(
    new ResourceIdentifier(
        ResourceType.Manifest,
        ResourceName.FromCode(1),
        new Language(1033)
    ),
    new byte[] {0x01, 0x02, 0x03}
);
```
<sup><a href='/Ressy.Tests/Snippets.cs#L85-L98' title='Snippet source file'>snippet source</a> | <a href='#snippet-setresource' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To remove all resources in a PE file, call the `ClearResources()` method:

<!-- snippet: ClearResources -->
<a id='snippet-clearresources'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.ClearResources();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L103-L109' title='Snippet source file'>snippet source</a> | <a href='#snippet-clearresources' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### High-level operations

Ressy provides extensions for `PortableExecutable` that enable you to directly read and manipulate known resource types, such as icons, manifests, versions, etc.

#### Manifest resources

A manifest resource (type 24) contains XML data that describes and identifies assemblies that an application should bind to at run time.
It may also contain other information, such as application settings, requested execution level, and more.

To learn more about application manifests, see [this article](https://docs.microsoft.com/en-us/windows/win32/sbscs/application-manifests).

##### Reading the manifest

To read the manifest resource as an XML text string, call the `GetManifest()` extension method:

<!-- snippet: GetManifest -->
<a id='snippet-getmanifest'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var manifest = portableExecutable.GetManifest();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L115-L121' title='Snippet source file'>snippet source</a> | <a href='#snippet-getmanifest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or: `TryGetManifest()`

<!-- snippet: TryGetManifest -->
<a id='snippet-trygetmanifest'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var manifest = portableExecutable.TryGetManifest();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L129-L135' title='Snippet source file'>snippet source</a> | <a href='#snippet-trygetmanifest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

> 💡 If there are multiple manifest resources, this method retrieves the one with the lowest ordinal name (ID), while giving preference to resources in the neutral language.
If there are no matching resources, this method retrieves the first manifest resource it finds.

##### Setting the manifest

To add or overwrite a manifest resource, call the `SetManifest(...)` extension method:

<!-- snippet: SetManifest -->
<a id='snippet-setmanifest'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetManifest("<assembly>...</assembly>");
```
<sup><a href='/Ressy.Tests/Snippets.cs#L142-L148' title='Snippet source file'>snippet source</a> | <a href='#snippet-setmanifest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

##### Removing the manifest

To remove all manifest resources, call the `RemoveManifest()` extension method:

<!-- snippet: RemoveManifest -->
<a id='snippet-removemanifest'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveManifest();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L153-L159' title='Snippet source file'>snippet source</a> | <a href='#snippet-removemanifest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Icon resources

Icon resources (type 3) and icon group resources (type 14) are used by the operating system to visually identify an application in the shell and other places.
Each portable executable file may contain multiple icon resources (usually in different sizes or color configurations), which are grouped together by the corresponding icon group resource.

##### Setting the icon

To add or overwrite icon resources based on an ICO file, call the `SetIcon(...)` extension method:

<!-- snippet: SetIcon -->
<a id='snippet-seticon'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetIcon("new_icon.ico");
```
<sup><a href='/Ressy.Tests/Snippets.cs#L164-L170' title='Snippet source file'>snippet source</a> | <a href='#snippet-seticon' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

> ⚠️ Calling this method does not remove existing icon and icon group resources, except for those that are overwritten directly.
If you want to clean out redundant icon resources (e.g. if the previous icon group contained more icons), call the `RemoveIcon()` method first.

Additionally, you can also set the icon by passing a stream that contains ICO-formatted data:

<!-- snippet: SetIconStream -->
<a id='snippet-seticonstream'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

using var iconFileStream = File.OpenRead("new_icon.ico");
portableExecutable.SetIcon(iconFileStream);
```
<sup><a href='/Ressy.Tests/Snippets.cs#L175-L182' title='Snippet source file'>snippet source</a> | <a href='#snippet-seticonstream' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

##### Removing the icon

To remove all icon and icon group resources, call the `RemoveIcon()` extension method:

<!-- snippet: RemoveIcon -->
<a id='snippet-removeicon'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.RemoveIcon();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L187-L193' title='Snippet source file'>snippet source</a> | <a href='#snippet-removeicon' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### Version info resources

A version info resource (type 16) contains file's version numbers, compatibility flags, and arbitrary string attributes.
Some of these attributes (such as, for example, `ProductName` and `Copyright`) are recognized by the operating system and may be displayed in some places.

##### Reading version info

To get the version info resource, call the `GetVersionInfo()` extension method.
This returns a `VersionInfo` object that represents the deserialized binary data stored in the resource:

<!-- snippet: GetVersionInfo -->
<a id='snippet-getversioninfo'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var versionInfo = portableExecutable.GetVersionInfo();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L199-L205' title='Snippet source file'>snippet source</a> | <a href='#snippet-getversioninfo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or `TryGetVersionInfo()`:

<!-- snippet: TryGetVersionInfo -->
<a id='snippet-trygetversioninfo'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

var versionInfo = portableExecutable.TryGetVersionInfo();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L213-L219' title='Snippet source file'>snippet source</a> | <a href='#snippet-trygetversioninfo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Returned object should contain:

<!-- snippet: Snippets.GetVersionInfo.verified.txt -->
<a id='snippet-Snippets.GetVersionInfo.verified.txt'></a>
```txt
{
  FileOperatingSystem: Windows32, WindowsNT,
  FileType: Application,
  AttributeTables: [
    {
      Language: {
        Id: Id_1
      },
      CodePage: {
        Id: Id_2
      },
      Attributes: {
        CompanyName: Microsoft Corporation,
        FileDescription: Notepad,
        InternalName: Notepad,
        LegalCopyright: © Microsoft Corporation. All rights reserved.,
        OriginalFilename: NOTEPAD.EXE.MUI,
        ProductName: Microsoft® Windows® Operating System,
      }
    }
  ]
}
```
<sup><a href='/Ressy.Tests/Snippets.GetVersionInfo.verified.txt#L1-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-Snippets.GetVersionInfo.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

> 💡 If there are multiple version info resources, this method retrieves the one with the lowest ordinal name (ID), while giving preference to resources in the neutral language.
If there are no matching resources, this method retrieves the first version info resource it finds.

When working with version info resources that include multiple attribute tables (bound to different language and code page pairs), you can use the `GetAttribute(...)` method to query a specific attribute.
This method searches through all attribute tables (while giving priority to tables in the neutral language) and returns the first matching value it finds:

<!-- snippet: GetAttribute -->
<a id='snippet-getattribute'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
var versionInfo = portableExecutable.GetVersionInfo();
var companyName = versionInfo.GetAttribute(VersionAttributeName.CompanyName);
// Microsoft Corporation
```
<sup><a href='/Ressy.Tests/Snippets.cs#L227-L234' title='Snippet source file'>snippet source</a> | <a href='#snippet-getattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or `TryGetAttribute`:

<!-- snippet: TryGetAttribute -->
<a id='snippet-trygetattribute'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
var versionInfo = portableExecutable.GetVersionInfo();
var companyName = versionInfo.TryGetAttribute(VersionAttributeName.CompanyName);
// Microsoft Corporation
```
<sup><a href='/Ressy.Tests/Snippets.cs#L242-L249' title='Snippet source file'>snippet source</a> | <a href='#snippet-trygetattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

##### Removing version info

To remove all version info resources, call the `RemoveVersionInfo()` extension method:

<!-- snippet: RemoveVersionInfo -->
<a id='snippet-removeversioninfo'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
portableExecutable.RemoveVersionInfo();
```
<sup><a href='/Ressy.Tests/Snippets.cs#L256-L261' title='Snippet source file'>snippet source</a> | <a href='#snippet-removeversioninfo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

##### Setting version info

To add or overwrite a version info resource, call the `SetVersionInfo(...)` extension method.
You can use the `VersionInfoBuilder` class to drastically simplify the creation of a new `VersionInfo` instance:

<!-- snippet: SetVersionInfo -->
<a id='snippet-setversioninfo'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

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
<sup><a href='/Ressy.Tests/Snippets.cs#L266-L281' title='Snippet source file'>snippet source</a> | <a href='#snippet-setversioninfo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also use an alternative overload of this method, which lets you selectively modify only a subset of properties in a version info resource, leaving the rest intact.
Properties that are not provided are pulled from the existing version info resource or resolved to their default values in case the resource does not exist:

<!-- snippet: SelectiveSetVersionInfo -->
<a id='snippet-selectivesetversioninfo'></a>
```cs
var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

portableExecutable.SetVersionInfo(v => v
    .SetFileVersion(new Version(1, 2, 3, 4))
    .SetAttribute("Custom Attribute", "My new value")
);
```
<sup><a href='/Ressy.Tests/Snippets.cs#L286-L295' title='Snippet source file'>snippet source</a> | <a href='#snippet-selectivesetversioninfo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

> 💡 When using the `SetAttribute(...)` method on `VersionInfoBuilder`, you can optionally specify the language and code page of the table that you want to add the attribute to.
If you choose to omit these parameters, Ressy will set the attribute in all attribute tables.
In case there are no existing attribute tables, this method creates a new one bound to the neutral language and Unicode code page.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ressy;
using Ressy.HighLevel.Icons;
using Ressy.HighLevel.Manifests;
using Ressy.HighLevel.Versions;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class Snippets
{
    static Snippets()
    {
        VerifierSettings.ScrubLinesContaining("Version");
        VerifierSettings.ModifySerialization(settings => settings.DontScrubNumericIds());
    }

    [Fact]
    public Task EnumeratingIdentifiers()
    {
        #region EnumeratingIdentifiers

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var identifiers = portableExecutable.GetResourceIdentifiers();

        #endregion

        var target = identifiers.Take(10)
            .Select(x => $"- Type: {x.Type.Label} {x.Language.Id}, Name: {x.Name.Code}, Language: {x.Language.Id}");
        return Verifier.Verify(target);
    }

    [Fact]
    public Task TryGetResource()
    {
        #region TryGetResource

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var resource = portableExecutable.TryGetResource(new ResourceIdentifier(
            ResourceType.Manifest,
            ResourceName.FromCode(100),
            new Language(1033)
        )); // resource is null

        #endregion

        return Verifier.Verify(resource);
    }

    [Fact]
    public Task RetrievingData()
    {
        #region RetrievingData

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var resource = portableExecutable.GetResource(new ResourceIdentifier(
            ResourceType.Manifest,
            ResourceName.FromCode(1),
            new Language(1033)
        ));

        var resourceData = resource.Data; // byte[]
        var resourceString = resource.ReadAsString(Encoding.UTF8); // string

        #endregion

        return Verifier.Verify(
            new
            {
                resourceData,
                resourceString
            });
    }

    void SetResource()
    {
        #region SetResource

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.SetResource(
            new ResourceIdentifier(
                ResourceType.Manifest,
                ResourceName.FromCode(1),
                new Language(1033)
            ),
            new byte[] {0x01, 0x02, 0x03}
        );

        #endregion
    }

    void ClearResources()
    {
        #region ClearResources

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.ClearResources();

        #endregion
    }

    [Fact]
    public Task GetManifest()
    {
        #region GetManifest

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var manifest = portableExecutable.GetManifest();

        #endregion

        return Verifier.Verify(manifest);
    }

    [Fact]
    public Task TryGetManifest()
    {
        #region TryGetManifest

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var manifest = portableExecutable.TryGetManifest();

        #endregion

        return Verifier.Verify(manifest);
    }

    void SetManifest()
    {
        #region SetManifest

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.SetManifest("<assembly>...</assembly>");

        #endregion
    }

    void RemoveManifest()
    {
        #region RemoveManifest

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.RemoveManifest();

        #endregion
    }

    void SetIcon()
    {
        #region SetIcon

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.SetIcon("new_icon.ico");

        #endregion
    }

    void SetIconStream()
    {
        #region SetIconStream

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        using var iconFileStream = File.OpenRead("new_icon.ico");
        portableExecutable.SetIcon(iconFileStream);

        #endregion
    }

    void RemoveIcon()
    {
        #region RemoveIcon

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.RemoveIcon();

        #endregion
    }

    [Fact]
    public Task GetVersionInfo()
    {
        #region GetVersionInfo

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var versionInfo = portableExecutable.GetVersionInfo();

        #endregion

        return Verifier.Verify(versionInfo);
    }

    [Fact]
    public Task TryGetVersionInfo()
    {
        #region TryGetVersionInfo

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        var versionInfo = portableExecutable.TryGetVersionInfo();

        #endregion

        return Verifier.Verify(versionInfo);
    }

    [Fact]
    public void GetAttribute()
    {
        #region GetAttribute

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
        var versionInfo = portableExecutable.GetVersionInfo();
        var companyName = versionInfo.GetAttribute(VersionAttributeName.CompanyName);
        // Microsoft Corporation

        #endregion

        Assert.Equal("Microsoft Corporation", companyName);
    }

    [Fact]
    public void TryGetAttribute()
    {
        #region TryGetAttribute

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
        var versionInfo = portableExecutable.GetVersionInfo();
        var companyName = versionInfo.TryGetAttribute(VersionAttributeName.CompanyName);
        // Microsoft Corporation

        #endregion

        Assert.Equal("Microsoft Corporation", companyName);
    }

    void RemoveVersionInfo()
    {
        #region RemoveVersionInfo

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");
        portableExecutable.RemoveVersionInfo();

        #endregion
    }

    void SetVersionInfo()
    {
        #region SetVersionInfo

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

        #endregion
    }

    void SelectiveSetVersionInfo()
    {
        #region SelectiveSetVersionInfo

        var portableExecutable = new PortableExecutable("C:/Windows/System32/notepad.exe");

        portableExecutable.SetVersionInfo(v => v
            .SetFileVersion(new Version(1, 2, 3, 4))
            .SetAttribute("Custom Attribute", "My new value")
        );

        #endregion
    }
}
using System.Linq;
using FluentAssertions;
using Ressy.Identification;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public record ManifestSpecs(DummyFixture DummyFixture) : IClassFixture<DummyFixture>
    {
        [Fact]
        public void User_can_read_content_of_an_application_manifest()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            var manifest = PortableExecutable.GetApplicationManifest(imageFilePath);

            // Assert
            manifest.Should().Contain("assemblyIdentity");
        }

        [Fact]
        public void User_can_add_an_application_manifest()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithoutResources();

            const string manifest = @"
                <?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
                <assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
                    <assemblyIdentity
                        name=""MyAssembly""
                        processorArchitecture=""x86""
                        version=""1.0.0.0""
                        type=""win32""/>
                </assembly>";

            // Act
            PortableExecutable.SetApplicationManifest(imageFilePath, manifest);

            // Assert
            PortableExecutable.GetApplicationManifest(imageFilePath).Should().Be(manifest);
        }

        [Fact]
        public void User_can_remove_an_application_manifest()
        {
            // Arrange
            var imageFilePath = DummyFixture.CreatePortableExecutableWithResources();

            // Act
            PortableExecutable.RemoveApplicationManifest(imageFilePath);

            // Assert
            PortableExecutable
                .GetResources(imageFilePath)
                .Where(r => r.Type.Code == (int)StandardResourceTypeCode.Manifest)
                .Should()
                .BeEmpty();
        }
    }
}
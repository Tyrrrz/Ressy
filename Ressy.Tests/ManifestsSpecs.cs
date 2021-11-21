using System.Linq;
using FluentAssertions;
using Ressy.Abstractions.Manifests;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public class ManifestsSpecs : IClassFixture<DummyFixture>
    {
        private readonly DummyFixture _dummy;

        public ManifestsSpecs(DummyFixture dummy) => _dummy = dummy;

        [Fact]
        public void User_can_get_the_application_manifest()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            var manifest = portableExecutable.GetManifest();

            // Assert
            manifest.Should().Contain("assemblyIdentity");
        }

        [Fact]
        public void User_can_add_an_application_manifest()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithoutResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

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
            portableExecutable.SetManifest(manifest);

            // Assert
            portableExecutable.GetManifest().Should().Be(manifest);
        }

        [Fact]
        public void User_can_remove_the_application_manifest()
        {
            // Arrange
            var imageFilePath = _dummy.CreatePortableExecutableWithResources();
            var portableExecutable = new PortableExecutable(imageFilePath);

            // Act
            portableExecutable.RemoveManifest();

            // Assert
            portableExecutable
                .GetResourceIdentifiers()
                .Where(r => r.Type.Code == (int)StandardResourceTypeCode.Manifest)
                .Should()
                .BeEmpty();
        }
    }
}
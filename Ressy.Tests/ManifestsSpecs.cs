using FluentAssertions;
using Ressy.HighLevel.Manifests;
using Ressy.Tests.Fixtures;
using Xunit;

namespace Ressy.Tests
{
    public class ManifestsSpecs : IClassFixture<DummyFixture>
    {
        private readonly DummyFixture _dummy;

        public ManifestsSpecs(DummyFixture dummy) => _dummy = dummy;

        [Fact]
        public void User_can_get_the_manifest()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            var manifest = portableExecutable.GetManifest();

            // Assert
            manifest.Should().Contain("assemblyIdentity");
        }

        [Fact]
        public void User_can_add_a_manifest()
        {
            // Arrange
            const string manifest = @"
                <?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
                <assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
                    <assemblyIdentity
                        name=""MyAssembly""
                        processorArchitecture=""x86""
                        version=""1.0.0.0""
                        type=""win32""/>
                </assembly>";

            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());
            portableExecutable.RemoveManifest();

            // Act
            portableExecutable.SetManifest(manifest);

            // Assert
            portableExecutable.GetManifest().Should().Be(manifest);
        }

        [Fact]
        public void User_can_remove_the_manifest()
        {
            // Arrange
            var portableExecutable = new PortableExecutable(_dummy.CreatePortableExecutable());

            // Act
            portableExecutable.RemoveManifest();

            // Assert
            portableExecutable.GetResourceIdentifiers().Should().NotContain(
                r => r.Type.Code == ResourceType.Manifest.Code
            );
        }
    }
}
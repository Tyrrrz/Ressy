using System;
using System.IO;
using Ressy.Tests.Utils;

namespace Ressy.Tests.Fixtures
{
    public class DummyFixture : IDisposable
    {
        private string SourceDirPath { get; } = Path.Combine(
            Path.GetDirectoryName(typeof(DummyFixture).Assembly.Location) ??
            Directory.GetCurrentDirectory()
        );

        private string StorageDirPath => Path.Combine(SourceDirPath, "Dummies");

        private string CreatePortableExecutable(string sourceFileName)
        {
            var sourceFilePath = Path.Combine(SourceDirPath, sourceFileName);

            Directory.CreateDirectory(StorageDirPath);
            var filePath = Path.Combine(StorageDirPath, $"PE-{Guid.NewGuid().GetHashCode()}.exe");
            File.Copy(sourceFilePath, filePath);

            return filePath;
        }

        public string CreatePortableExecutableWithoutResources() => CreatePortableExecutable("SmallPEpe.exe");

        public string CreatePortableExecutableWithResources() => CreatePortableExecutable("LargePEpe.exe");

        public void Dispose() => DirectoryEx.DeleteIfExists(StorageDirPath);
    }
}
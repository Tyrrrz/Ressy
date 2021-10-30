using System;
using System.IO;
using System.Threading;
using Ressy.Tests.Utils;

namespace Ressy.Tests.Fixtures
{
    public class DummyFixture : IDisposable
    {
        private readonly int _id = Guid.NewGuid().GetHashCode();
        private int _dummyCount;

        private string SourceDirPath { get; } = Path.Combine(
            Path.GetDirectoryName(typeof(DummyFixture).Assembly.Location) ??
            Directory.GetCurrentDirectory()
        );

        private string StorageDirPath => Path.Combine(SourceDirPath, $"Dummies_{_id}");

        private string CreatePortableExecutable(string sourceFileName)
        {
            var sourceFilePath = Path.Combine(SourceDirPath, sourceFileName);

            Directory.CreateDirectory(StorageDirPath);

            var fileIndex = Interlocked.Increment(ref _dummyCount);
            var filePath = Path.Combine(StorageDirPath, $"PE-{fileIndex}.exe");

            File.Copy(sourceFilePath, filePath);

            return filePath;
        }

        public string CreatePortableExecutableWithoutResources() => CreatePortableExecutable("SmallPEpe.exe");

        public string CreatePortableExecutableWithResources() => CreatePortableExecutable("LargePEpe.exe");

        public void Dispose() => DirectoryEx.DeleteIfExists(StorageDirPath);
    }
}
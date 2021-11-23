using System.IO;
using System.Linq;
using System.Text;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Versions
{
    public partial class VersionInfo
    {
        private long WriteFixedFileInfo(BinaryWriter writer)
        {
            var startPosition = writer.BaseStream.Position;

            // dwSignature
            writer.Write(0xFEEF04BD);

            // dwStrucVersion
            writer.Write(0x00010000);

            // dwFileVersionMS, dwFileVersionLS
            var fileVersionClamped = FileVersion.ClampComponentsAboveZero();
            writer.Write(BitPack.Merge((ushort)fileVersionClamped.Major, (ushort)fileVersionClamped.Minor));
            writer.Write(BitPack.Merge((ushort)fileVersionClamped.Build, (ushort)fileVersionClamped.Revision));

            // dwProductVersionMS, dwProductVersionLS
            var productVersionClamped = ProductVersion.ClampComponentsAboveZero();
            writer.Write(BitPack.Merge((ushort)productVersionClamped.Major, (ushort)productVersionClamped.Minor));
            writer.Write(BitPack.Merge((ushort)productVersionClamped.Build, (ushort)productVersionClamped.Revision));

            // dwFileFlagsMask
            writer.Write(0x3F);

            // dwFileFlags
            writer.Write((uint)FileFlags);

            // dwFileOS
            writer.Write((uint)FileOperatingSystem);

            // dwFileType
            writer.Write((uint)FileType);

            // dwFileSubtype
            writer.Write((uint)FileSubType);

            // dwFileDateMS, dwFileDateLS
            writer.Write(FileTimestamp.ToFileTime());

            return writer.BaseStream.Position - startPosition;
        }

        private long WriteStringFileInfo(BinaryWriter writer)
        {
            // wLength (will overwrite later)
            var lengthCheckpoint = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wValueLength (always zero)
            writer.Write((ushort)0);

            // wType
            writer.Write((ushort)1);

            // szKey
            writer.WriteStringNullTerminated("StringFileInfo");

            // Padding
            writer.SkipPadding();

            // -- StringTable
            if (Attributes.Any())
            {
                // wLength (will overwrite later)
                var tableLengthCheckpoint = writer.BaseStream.CreatePortal();
                writer.Write((ushort)0);

                // wValueLength (always zero)
                writer.Write((ushort)0);

                // wType
                writer.Write((ushort)1);

                // szKey
                writer.WriteStringNullTerminated("040904B0");

                // Children
                foreach (var (name, value) in Attributes)
                {
                    // -- String

                    // Padding
                    writer.SkipPadding();

                    // wLength (will overwrite later)
                    var stringLengthCheckpoint = writer.BaseStream.CreatePortal();
                    writer.Write((ushort)0);

                    // wValueLength
                    writer.Write((ushort)value.Length);

                    // wType
                    writer.Write((ushort)1);

                    // szKey
                    writer.WriteStringNullTerminated(name);

                    // Padding
                    writer.SkipPadding();

                    // Value
                    writer.WriteStringNullTerminated(value);

                    // Update length
                    var stringLength = writer.BaseStream.Position - stringLengthCheckpoint.Position;
                    using (stringLengthCheckpoint.Jump())
                        writer.Write((ushort)stringLength);
                }

                // Update length
                var tableLength = writer.BaseStream.Position - tableLengthCheckpoint.Position;
                using (tableLengthCheckpoint.Jump())
                    writer.Write((ushort)tableLength);
            }

            // Update length
            var length = writer.BaseStream.Position - lengthCheckpoint.Position;
            using (lengthCheckpoint.Jump())
                writer.Write((ushort)length);

            return length;
        }

        private long WriteVarFileInfo(BinaryWriter writer)
        {
            if (!Translations.Any())
                return 0;

            // wLength (will overwrite later)
            var lengthCheckpoint = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wValueLength (always zero)
            writer.Write((ushort)0);

            // wType
            writer.Write((ushort)1);

            // szKey
            writer.WriteStringNullTerminated("VarFileInfo");

            // Padding
            writer.SkipPadding();

            // -- Translation

            // wLength (will overwrite later)
            var translationLength = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wValueLength (always zero)
            writer.Write((ushort)0);

            // wType
            writer.Write((ushort)1);

            // szKey
            writer.WriteStringNullTerminated("Translation");

            // Children
            foreach (var translation in Translations)
            {
                // -- Var

                // Padding
                writer.SkipPadding();

                // Value
                writer.Write(BitPack.Merge((ushort)translation.Codepage, (ushort)translation.LanguageId));
            }

            // Update length
            var varFileInfoLength = writer.BaseStream.Position - translationLength.Position;
            using (translationLength.Jump())
                writer.Write((ushort)varFileInfoLength);

            // Update length
            var length = writer.BaseStream.Position - lengthCheckpoint.Position;
            using (lengthCheckpoint.Jump())
                writer.Write((ushort)length);

            return length;
        }

        internal byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.Unicode);

            // -- VS_VERSIONINFO

            // wLength (will overwrite later)
            var lengthCheckpoint = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wValueLength (will overwrite later)
            var fixedFileInfoLengthCheckpoint = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wType
            writer.Write((ushort)0);

            // szKey
            writer.WriteStringNullTerminated("VS_VERSION_INFO");

            // Padding
            writer.SkipPadding();

            // -- VS_FIXEDFILEINFO
            var fixedFileInfoLength = WriteFixedFileInfo(writer);
            using (fixedFileInfoLengthCheckpoint.Jump())
                writer.Write((ushort)fixedFileInfoLength);

            // Padding
            writer.SkipPadding();

            // -- StringFileInfo
            WriteStringFileInfo(writer);

            // -- VarFileInfo
            WriteVarFileInfo(writer);

            // Update length
            var length = stream.Position - lengthCheckpoint.Position;
            using (lengthCheckpoint.Jump())
                writer.Write((ushort)length);

            return stream.ToArray();
        }
    }
}
using System.IO;
using System.Linq;
using System.Text;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.Abstractions.Versions
{
    public partial class VersionInfo
    {
        internal byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.Unicode);

            // -- VS_VERSIONINFO

            // wLength (will overwrite later)
            var rootLengthPosition = stream.Position;
            writer.Write((ushort)0);

            // wValueLength (will overwrite later)
            writer.Write((ushort)0);

            // wType
            writer.Write((ushort)0);

            // szKey
            writer.WriteStringNullTerminated("VS_VERSION_INFO");

            // Padding
            writer.SkipPadding();

            // -- VS_FIXEDFILEINFO

            var fixedFileInfoPosition = stream.Position;

            // dwSignature
            writer.Write(0xFEEF04BD);

            // dwStrucVersion
            writer.Write(0x00010000);

            // dwFileVersionMS, dwFileVersionLS
            writer.Write(BitPack.Merge((ushort)FileVersion.Major, (ushort)FileVersion.Minor));
            writer.Write(BitPack.Merge((ushort)FileVersion.Build, (ushort)FileVersion.Revision));

            // dwProductVersionMS, dwProductVersionLS
            writer.Write(BitPack.Merge((ushort)ProductVersion.Major, (ushort)ProductVersion.Minor));
            writer.Write(BitPack.Merge((ushort)ProductVersion.Build, (ushort)ProductVersion.Revision));

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

            // Update root value length
            var fixedFileInfoLength = stream.Position - fixedFileInfoPosition;
            using (writer.BaseStream.JumpAndReturn(2))
                writer.Write((ushort)fixedFileInfoLength);

            // Padding
            writer.SkipPadding();

            // -- StringFileInfo

            // wLength (will overwrite later)
            var stringFileInfoLengthPosition = stream.Position;
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
                var stringTableLengthPosition = stream.Position;
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
                    var stringLengthPosition = stream.Position;
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
                    var stringLength = stream.Position - stringLengthPosition;
                    using (writer.BaseStream.JumpAndReturn(stringLengthPosition))
                        writer.Write((ushort)stringLength);
                }

                // Update string table length
                var stringTableLength = stream.Position - stringTableLengthPosition;
                using (writer.BaseStream.JumpAndReturn(stringTableLengthPosition))
                    writer.Write((ushort)stringTableLength);

                // Update string file info length
                var stringFileInfoLength = stream.Position - stringFileInfoLengthPosition;
                using (writer.BaseStream.JumpAndReturn(stringFileInfoLengthPosition))
                    writer.Write((ushort)stringFileInfoLength);
            }

            // -- VarFileInfo
            if (Translations.Any())
            {
                // wLength (will overwrite later)
                var varFileInfoLengthPosition = stream.Position;
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
                var translationLengthPosition = stream.Position;
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

                // Update translation length
                var translationLength = stream.Position - translationLengthPosition;
                using (writer.BaseStream.JumpAndReturn(translationLengthPosition))
                    writer.Write((ushort)translationLength);

                // Update var file info length
                var varFileInfoLength = stream.Position - varFileInfoLengthPosition;
                using (writer.BaseStream.JumpAndReturn(varFileInfoLengthPosition))
                    writer.Write((ushort)varFileInfoLength);
            }

            // Update root length
            var rootLength = stream.Position - rootLengthPosition;
            using (writer.BaseStream.JumpAndReturn(rootLengthPosition))
                writer.Write((ushort)rootLength);

            return stream.ToArray();
        }
    }
}
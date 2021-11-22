using System.IO;
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

            // Padding
            writer.SkipPadding();

            // -- StringFileInfo

            // wLength (will overwrite later)
            var stringFileInfoLengthOffset = stream.Position;
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

            // wLength (will overwrite later)
            var stringTableLengthOffset = stream.Position;
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
            }

            // Update string table length
            var stringTableLength = stream.Position - stringTableLengthOffset;
            using (writer.BaseStream.JumpAndReturn(stringTableLengthOffset))
                writer.Write((ushort)stringTableLength);

            // Update string file info length
            var stringFileInfoLength = stream.Position - stringFileInfoLengthOffset;
            using (writer.BaseStream.JumpAndReturn(stringFileInfoLengthOffset))
                writer.Write((ushort)stringFileInfoLength);

            // -- VarFileInfo

            // wLength (will overwrite later)
            var varFileInfoLengthOffset = stream.Position;
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
            var translationLengthOffset = stream.Position;
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
            var translationLength = stream.Position - translationLengthOffset;
            using (writer.BaseStream.JumpAndReturn(translationLengthOffset))
                writer.Write((ushort)translationLength);

            // Update var file info length
            var varFileInfoLength = stream.Position - varFileInfoLengthOffset;
            using (writer.BaseStream.JumpAndReturn(varFileInfoLengthOffset))
                writer.Write((ushort)varFileInfoLength);

            return stream.ToArray();
        }
    }
}
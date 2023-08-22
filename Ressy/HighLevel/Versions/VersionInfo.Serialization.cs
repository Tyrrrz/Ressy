using System.Globalization;
using System.IO;
using Ressy.Utils;
using Ressy.Utils.Extensions;

namespace Ressy.HighLevel.Versions;

public partial class VersionInfo
{
    private void WriteFixedFileInfo(BinaryWriter writer)
    {
        // dwSignature
        writer.Write(0xFEEF04BD);

        // dwStrucVersion
        writer.Write(0x00010000);

        // dwFileVersionMS, dwFileVersionLS
        var fileVersionClamped = FileVersion.ClampComponents();

        writer.Write(
            BitPack.Merge((ushort)fileVersionClamped.Major, (ushort)fileVersionClamped.Minor)
        );

        writer.Write(
            BitPack.Merge((ushort)fileVersionClamped.Build, (ushort)fileVersionClamped.Revision)
        );

        // dwProductVersionMS, dwProductVersionLS
        var productVersionClamped = ProductVersion.ClampComponents();

        writer.Write(
            BitPack.Merge((ushort)productVersionClamped.Major, (ushort)productVersionClamped.Minor)
        );

        writer.Write(
            BitPack.Merge(
                (ushort)productVersionClamped.Build,
                (ushort)productVersionClamped.Revision
            )
        );

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

        // dwFileDateMS, dwFileDateLS (never actually used by Win32)
        writer.Write((ulong)0L);
    }

    private void WriteStringFileInfo(BinaryWriter writer)
    {
        // wLength (will overwrite later)
        var lengthPortal = writer.BaseStream.CreatePortal();
        writer.Write((ushort)0);

        // wValueLength (always zero)
        writer.Write((ushort)0);

        // wType (always zero)
        writer.Write((ushort)0);

        // szKey
        writer.WriteNullTerminatedString("StringFileInfo");

        // Padding
        writer.SkipPadding();

        // -- StringTable

        foreach (var attributeTable in AttributeTables)
        {
            // wLength (will overwrite later)
            var tableLengthPortal = writer.BaseStream.CreatePortal();
            writer.Write((ushort)0);

            // wValueLength (always zero)
            writer.Write((ushort)0);

            // wType (always zero)
            writer.Write((ushort)0);

            // szKey
            writer.WriteNullTerminatedString(
                attributeTable.Language.Id.ToString("X4", CultureInfo.InvariantCulture)
                    + attributeTable.CodePage.Id.ToString("X4", CultureInfo.InvariantCulture)
            );

            // -- String
            foreach (var (name, value) in attributeTable.Attributes)
            {
                // Padding
                writer.SkipPadding();

                // wLength (will overwrite later)
                var stringLengthPortal = writer.BaseStream.CreatePortal();
                writer.Write((ushort)0);

                // wValueLength (includes null terminator)
                writer.Write((ushort)Encoding.GetByteCount(value + '\0'));

                // wType (always one)
                writer.Write((ushort)1);

                // szKey
                writer.WriteNullTerminatedString(name);

                // Padding
                writer.SkipPadding();

                // Value
                writer.WriteNullTerminatedString(value);

                // Update length
                var stringLength = writer.BaseStream.Position - stringLengthPortal.Position;
                using (stringLengthPortal.Jump())
                    writer.Write((ushort)stringLength);
            }

            // Update length
            var tableLength = writer.BaseStream.Position - tableLengthPortal.Position;
            using (tableLengthPortal.Jump())
                writer.Write((ushort)tableLength);
        }

        // Update length
        var length = writer.BaseStream.Position - lengthPortal.Position;
        using (lengthPortal.Jump())
            writer.Write((ushort)length);
    }

    private void WriteVarFileInfo(BinaryWriter writer)
    {
        // wLength (will overwrite later)
        var lengthPortal = writer.BaseStream.CreatePortal();
        writer.Write((ushort)0);

        // wValueLength (always zero)
        writer.Write((ushort)0);

        // wType (always zero)
        writer.Write((ushort)0);

        // szKey
        writer.WriteNullTerminatedString("VarFileInfo");

        // Padding
        writer.SkipPadding();

        // -- Translation

        // wLength (will overwrite later)
        var translationLengthPortal = writer.BaseStream.CreatePortal();
        writer.Write((ushort)0);

        // wValueLength (4 bytes per codepage*languageId pair)
        writer.Write((ushort)(AttributeTables.Count * 4));

        // wType (always zero)
        writer.Write((ushort)0);

        // szKey
        writer.WriteNullTerminatedString("Translation");

        // Padding
        writer.SkipPadding();

        // -- Var
        foreach (var attributeTable in AttributeTables)
        {
            writer.Write(
                BitPack.Merge(
                    (ushort)attributeTable.CodePage.Id,
                    (ushort)attributeTable.Language.Id
                )
            );
        }

        // Update length
        var varFileInfoLength = writer.BaseStream.Position - translationLengthPortal.Position;
        using (translationLengthPortal.Jump())
            writer.Write((ushort)varFileInfoLength);

        // Update length
        var length = writer.BaseStream.Position - lengthPortal.Position;
        using (lengthPortal.Jump())
            writer.Write((ushort)length);
    }

    internal byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding);

        // -- VS_VERSIONINFO

        // wLength (will overwrite later)
        var lengthPortal = writer.BaseStream.CreatePortal();
        writer.Write((ushort)0);

        // wValueLength (always 52)
        writer.Write((ushort)52);

        // wType (always zero)
        writer.Write((ushort)0);

        // szKey
        writer.WriteNullTerminatedString("VS_VERSION_INFO");

        // Padding
        writer.SkipPadding();

        // -- VS_FIXEDFILEINFO
        WriteFixedFileInfo(writer);

        // Padding
        writer.SkipPadding();

        // -- StringFileInfo
        WriteStringFileInfo(writer);

        // Padding
        writer.SkipPadding();

        // -- VarFileInfo
        WriteVarFileInfo(writer);

        // Update length
        var length = stream.Position - lengthPortal.Position;
        using (lengthPortal.Jump())
            writer.Write((ushort)length);

        writer.Flush();

        return stream.ToArray();
    }
}

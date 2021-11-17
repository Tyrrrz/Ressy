using System.Globalization;
using Ressy.Native;

namespace Ressy
{
    internal class OrdinalResourceType : ResourceType
    {
        private readonly int _code;

        public override int? Code => _code;

        public override string Label
        {
            get
            {
                var codePortion = '#' + _code.ToString(CultureInfo.InvariantCulture);

                var standardTypePortion = (StandardResourceTypeCode)_code switch
                {
                    StandardResourceTypeCode.Cursor => "CURSOR",
                    StandardResourceTypeCode.Bitmap => "BITMAP",
                    StandardResourceTypeCode.Icon => "ICON",
                    StandardResourceTypeCode.Menu => "MENU",
                    StandardResourceTypeCode.Dialog => "DIALOG",
                    StandardResourceTypeCode.String => "STRING",
                    StandardResourceTypeCode.FontDir => "FONTDIR",
                    StandardResourceTypeCode.Font => "FONT",
                    StandardResourceTypeCode.Accelerator => "ACCELERATOR",
                    StandardResourceTypeCode.RawData => "RCDATA",
                    StandardResourceTypeCode.MessageTable => "MESSAGETABLE",
                    StandardResourceTypeCode.GroupCursor => "GROUP_CURSOR",
                    StandardResourceTypeCode.GroupIcon => "GROUP_ICON",
                    StandardResourceTypeCode.Version => "VERSION",
                    StandardResourceTypeCode.DlgInclude => "DLGINCLUDE",
                    StandardResourceTypeCode.PlugAndPlay => "PLUGPLAY",
                    StandardResourceTypeCode.Vxd => "VXD",
                    StandardResourceTypeCode.AnimatedCursor => "ANICURSOR",
                    StandardResourceTypeCode.AnimatedIcon => "ANIICON",
                    StandardResourceTypeCode.Html => "HTML",
                    StandardResourceTypeCode.Manifest => "MANIFEST",
                    _ => null
                };

                return !string.IsNullOrWhiteSpace(standardTypePortion)
                    ? codePortion + " (" + standardTypePortion + ")"
                    : codePortion;
            }
        }

        public OrdinalResourceType(int code) => _code = code;

        internal override SafeIntPtr ToPointer() => SafeIntPtr.FromValue(_code);
    }
}
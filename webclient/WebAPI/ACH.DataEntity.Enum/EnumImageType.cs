using System.ComponentModel;

namespace ACH.DataEntity.Enum
{
    public enum EnumImageType
    {
        [Description("data:image/svg+xml;charset=utf-8,")]
        SVG_BASE,
        [Description("data:image/png;base64,")]
        PNG_BASE,
        [Description("data:image/svg+xml;base64,")]
        SVG_UTF,
    }
}

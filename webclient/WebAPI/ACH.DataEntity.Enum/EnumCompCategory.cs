using System.ComponentModel;

namespace ACH.DataEntity.Enum
{
    /// <summary>
    /// 聚合部件类型
    /// </summary>
    public enum EnumCompCategory
    {
        [Description("传动链")]
        CVM,
        [Description("叶片")]
        BVM,
        [Description("塔筒")]
        TVM,
        [Description("法兰")]
        FLG,
        [Description("索力")]
        CBF,
    }
}

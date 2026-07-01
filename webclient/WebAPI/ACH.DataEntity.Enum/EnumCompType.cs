using System.ComponentModel;

namespace ACH.DataEntity.Enum
{
    /// <summary>
    /// 实体部件类型
    /// </summary>
    public enum EnumCompType
    {
        [Description("主轴")]
        MST,
        [Description("齿轮箱")]
        GBX,
        [Description("发电机")]
        GEN,
        [Description("塔筒")]
        TOW,
        [Description("叶片1")]
        BL1,
        [Description("叶片2")]
        BL2,
        [Description("叶片3")]
        BL3,
        [Description("变桨轴承1")]
        PB1,
        [Description("变桨轴承2")]
        PB2,
        [Description("变桨轴承3")]
        PB3,
    }
}

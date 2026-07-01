using System.ComponentModel;

namespace ACH.DataEntity.Enum
{
    public enum EnumDiagBaseType
    {
        [Description("表面损伤")]
        SurfaceDamage = 0,

        [Description("结构损伤")]
        StructureDamage = 1,

        [Description("覆冰")]
        Ice = 2,

        [Description("桨距角偏差")]
        PB = 3,

        [Description("基础倾斜预警")]
        Actrual = 4,

        [Description("塔筒顶部位移超限预警")]
        TDBAVG = 5,

        [Description("基础松动预警")]
        NFPiRo = 6,

    }
}

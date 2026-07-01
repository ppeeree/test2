using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ACH.DataEntity.Enum
{
    /// <summary>
    /// 分布圆类型枚举
    /// </summary>
    public enum EnumCircleType
    {
        [Description("法兰间隙")]
        FLG_GAP = 0,

        [Description("索力")]
        CBF = 1,

        [Description("法兰螺栓")]
        BFM = 2,

        [Description("其他")]
        Others = 99
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// DAU（ICP）传感器异常
    /// </summary>
    public enum EnumICPSensorStatus
    {
        [EnumMember]
        [Description("短路")]
        SHORT_OUT = 0,

        [EnumMember]
        [Description("断路")]
        DISCONNECTION = 1,

        [EnumMember]
        [Description("跳变")]
        JUMPING = 2,

        [EnumMember]
        [Description("直流分量异常")]
        DC_EXCEPTION = 3,

        [EnumMember]
        [Description("采集振动值偏小")]
        CollectValue_SMALL = 4,
        [EnumMember]
        [Description("正常")]
        Normal = 5,
    }
}

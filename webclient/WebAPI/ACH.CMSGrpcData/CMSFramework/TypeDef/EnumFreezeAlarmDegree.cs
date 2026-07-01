using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 覆冰报警等级
    /// </summary>
    [DataContract(Name = "EnumFreezeAlarmDegree")]
    [Obsolete("Please Use EnumAlarmDegree.", true)]
    public enum EnumFreezeAlarmDegree
    {
        [EnumMember]
        [Description("未知")]
        AlarmDeg_Unknown = 2,

        [EnumMember]
        [Description("无覆冰")]
        AlarmDeg_Normal = 3,

        [EnumMember]
        [Description("轻微覆冰")]
        AlarmDeg_ICECommon = 8,

        [EnumMember]
        [Description("严重覆冰")]
        AlarmDeg_ICESerious = 9,
    }
}

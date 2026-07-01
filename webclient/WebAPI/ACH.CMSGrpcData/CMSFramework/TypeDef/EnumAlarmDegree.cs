using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumAlarmDegree")]
    public enum EnumAlarmDegree
    {
        [EnumMember]
        [Description("占位")]
        AlarmDeg_Dummy = 0,

        [EnumMember]
        [Description("未知")]
        AlarmDeg_Unknown = 2,

        [EnumMember]
        [Description("正常")]
        AlarmDeg_Normal = 3,

        [EnumMember]
        [Description("系统异常")]
        AlarmDeg_SystemError = 4,

        [EnumMember]
        [Description("注意")]
        AlarmDeg_Warning = 5,

        [EnumMember]
        [Description("危险")]
        AlarmDeg_Alarm = 6,

        //[EnumMember]
        //[Description("轻微覆冰")]
        //AlarmDeg_ICECommon = 8,

        //[EnumMember]
        //[Description("严重覆冰")]
        //AlarmDeg_ICESerious = 9,

        //[EnumMember]
        //[Description("损伤")]
        //AlarmDeg_Harm = 10,
    }
}

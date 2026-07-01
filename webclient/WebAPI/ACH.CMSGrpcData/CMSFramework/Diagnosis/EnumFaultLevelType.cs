using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity.Diagnosis
{
    [DataContract]
    public enum EnumFaultLevelType
    {
        [EnumMember]
        [Description("正常")]
        Normal = 0,

        [EnumMember]
        [Description("注意")]
        Caution = 1,

        [EnumMember]
        [Description("警告")]
        Warning = 2,

        [EnumMember]
        [Description("报警")]
        Alarm = 3,

        [EnumMember]
        [Description("危险")]
        Danger = 4,

        /// <summary>
        /// 该机组未诊断，诊断结果未知
        /// </summary>
        [EnumMember]
        [Description("未诊断")]
        UnKnown = 5,
    }
}

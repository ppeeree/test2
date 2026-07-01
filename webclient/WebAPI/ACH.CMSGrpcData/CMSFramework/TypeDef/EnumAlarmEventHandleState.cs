using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumAlarmEventHandleState")]
    public enum EnumAlarmEventHandleState
    {
        /// <summary>
        /// 未处理
        /// </summary>
        [EnumMember]
        [Description("未处理")]
        Unhandled = 0,

        /// <summary>
        /// 诊断中
        /// </summary>
        [EnumMember]
        [Description("诊断中")]
        Diagnosis= 1,

        /// <summary>
        /// 维修中
        /// </summary>
        [EnumMember]
        [Description("维修中")]
        Maintenance = 2,

        /// <summary>
        /// 已完成
        /// </summary>
        [EnumMember]
        [Description("已完成")]
        Finished = 3,

        /// <summary>
        /// 已忽略
        /// </summary>
        [EnumMember]
        [Description("已忽略")]
        Ignore = 4

    }
}

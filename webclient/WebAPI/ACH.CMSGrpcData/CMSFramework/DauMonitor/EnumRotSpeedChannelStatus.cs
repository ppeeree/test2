using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumRotSpeedChannelStatus")]
    public enum EnumRotSpeedChannelStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        [EnumMember]
        [Description("未知")]
        Unknown = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [EnumMember]
        [Description("正常")]
        Normal = 1,
        

        /// <summary>
        /// 故障
        /// </summary>
        [EnumMember]
        [Description("故障")]
        Fault = 2,

        /// <summary>
        /// 未采集到数据
        /// </summary>
        [EnumMember]
        [Description("未采集到数据")]
        NotAcquisitionData = 3
    }
}

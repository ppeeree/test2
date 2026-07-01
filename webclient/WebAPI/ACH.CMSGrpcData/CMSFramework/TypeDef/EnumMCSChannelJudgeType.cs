using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author: Yangming
    // create: 2012-08-14
    /// <summary>
    /// 主控系统参数通道 状态寄存器值判断模式
    /// </summary>
    [DataContract(Name = "EnumMCSChannelJudgeType")]
    public enum EnumMCSChannelJudgeType
    {
        [EnumMember]
        [Description("等于")]
        Equal = 0,

        [EnumMember]
        [Description("不等于")]
        NotEqual = 1,

        [EnumMember]
        [Description("范围内")]
        InBound = 2,

        [EnumMember]
        [Description("范围外")]
        OutBound = 3
    }
}

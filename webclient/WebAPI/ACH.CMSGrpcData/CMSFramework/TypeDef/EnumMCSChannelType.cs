using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2011-06-22
    /// <summary>
    /// 测量定义类型
    /// </summary>
    [DataContract(Name = "EnumMCSChannelType")]
    public enum EnumMCSChannelType
    {
        [EnumMember]
        [Description("输入寄存器")]
        Input_Register = 0,

        [EnumMember]
        [Description("保持寄存器")]
        Hold_Register = 1

    }
}

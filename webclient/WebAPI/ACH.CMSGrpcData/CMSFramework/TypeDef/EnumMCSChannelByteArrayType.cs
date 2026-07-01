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
    /// 主控系统参数通道 字节序模式
    /// </summary>
    [DataContract(Name = "EnumMCSChannelByteArrayType")]
    public enum EnumMCSChannelByteArrayType
    {
        [EnumMember]
        [Description("大端")]
        Big_Endian = 0,

        [EnumMember]
        [Description("小端")]
        Little_Endian = 1
    }
}

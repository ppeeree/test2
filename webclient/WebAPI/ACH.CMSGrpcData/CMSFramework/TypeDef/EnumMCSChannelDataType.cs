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
    // create: 2012-04-27
    /// <summary>
    /// 主控系统参数通道 数据类型
    /// </summary>
    [DataContract(Name = "EnumMCSChannelDataType")]
    public enum EnumMCSChannelDataType
    {
        [EnumMember]
        [Description("数据")]
        Data = 0 ,

        [EnumMember]
        [Description("状态")]
        Status = 1
  
    } 
}

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
    /// 主控系统参数通道 数据存储方式
    /// </summary>
    [DataContract(Name = "EnumMCSChannelStorageType")]
    public enum EnumMCSChannelStorageType
    {
        [EnumMember]
        [Description("16位整数")]
        DataInteger = 0 ,

        [EnumMember]
        [Description("32位浮点数")]
        DataFloat = 1
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author: GuoKaile
    // create: 2012-06-22
    /// <summary>
    /// 过程量数据来源
    /// </summary>
    [DataContract(Name = "EnumWorkConDataSource")]
    public enum EnumWorkConDataSource
    {
        [EnumMember]
        [Description("主控系统")]
        ModbusOnTcp = 0,

        [EnumMember]
        [Description("Wind DAU")]
        WindDAU = 1,

#if NGC
        /// <summary>
        /// 因南高齿诊断平台使用而新增
        /// </summary>
        [EnumMember]
        [Description("GearSight")]
        GearSight = 2
#endif
    }
}

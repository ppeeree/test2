using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 历史数据类型
    /// </summary>
    [DataContract(Name = "EnumDataSource")]
    public enum EnumDataSource
    {
        /// <summary>
        /// 报警库
        /// </summary>
        [EnumMember]
        [Description("AlarmDB")]
        AlarmDB = 1,
        /// <summary>
        /// 历史库
        /// </summary>
        [EnumMember]
        [Description("HisDB")]
        HisDB = 2,
        /// <summary>
        /// 天库
        /// </summary>
        [EnumMember]
        [Description("DayDB")]
        DayDB = 3,
        /// <summary>
        /// 小时库
        /// </summary>
        [EnumMember]
        [Description("HourDB")]
        HourDB = 4,

        /// <summary>
        /// 实时数据文件库
        /// </summary>
        [EnumMember]
        [Description("RealTimeDB")]
        RealTimeDB = 5,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    
    [Obsolete("Remove from the Framework, It is used only by Storage Module.", false)]
    public class RTWFDataTableUpdateRecord
    {
        /// <summary>
        /// 报警状态
        /// </summary>
        [DataMember]
        public int AlamState { get; set; }
        /// <summary>
        /// 测量定义ID
        /// </summary>
        [DataMember]
        public string MeasDefinitionID { get; set; }
        /// <summary>
        /// 报警库更新时间
        /// </summary>
        [DataMember]
        public DateTime UpdateTimeAlarm { get; set; }
        /// <summary>
        /// 天库更新时间
        /// </summary>
        [DataMember]
        public DateTime UpdateTimeDay { get; set; }
        /// <summary>
        /// 特征值更新时间
        /// </summary>
        [DataMember]
        public DateTime UpdateTimeEig { get; set; }
        /// <summary>
        /// 历史库更新时间
        /// </summary>
        [DataMember]
        public DateTime UpdateTimeHis { get; set; }
        /// <summary>
        /// 小时库更新时间
        /// </summary>
        [DataMember]
        public DateTime UpdateTimeHour { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID { get; set; }
        /// <summary>
        /// 工况等级
        /// </summary>
        [DataMember]
        public int WorkCondDegree { get; set; }
    }
}

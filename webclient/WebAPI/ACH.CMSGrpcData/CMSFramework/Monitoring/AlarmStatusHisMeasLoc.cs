using CMSFramework.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 测量位置报警历史
    /// </summary>
    public class AlarmStatusHisMeasLoc
    {
        /// <summary>
        /// 报警等级
        /// </summary>
        public EnumAlarmDegree AlarmDegree { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime AlarmTime { get; set; }
        /// <summary>
        /// 部件ID
        /// </summary>
        public string ComponentId { get; set; }
        /// <summary>
        /// 测量位置
        /// </summary>
        public string MeasLocId { get; set; }
        /// <summary>
        /// 测量位置名称
        /// </summary>
        public string MeasLocName { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbineId { get; set; }
        /// <summary>
        /// 机组名称
        /// </summary>
        public string WindTurbineName { get; set; }
        /// 风场ID
        /// </summary>
        /// <summary>
        public string WindParkId { get; set; }


    }
}

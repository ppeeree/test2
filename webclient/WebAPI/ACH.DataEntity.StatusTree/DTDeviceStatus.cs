using ACH.DataEntity.Common;
using System;
using System.Collections.Generic;

namespace ACH.DataEntity.StatusTree
{
    /// <summary>
    /// 构建设备树 - 机组层 （DT表示DeviceTree，设备树）
    /// </summary>
    public class DTDeviceStatus
    {
        /// <summary>
        /// 机组Code
        /// </summary>
        public string WindTurbuineCode { get; set; }

        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbuineId { get; set; }

        /// <summary>
        /// 机组名称
        /// </summary>
        public string WindTurbuineName { get; set; }

        /// <summary>
        /// 组机状态
        /// </summary>
        public EnumAlarmStatus WindTurbuineStatus
        {
            get; set;
            /*get
            {
                if (CompList == null || CompList.Count == 0)
                {
                    return EnumAlarmStatus.Nostate;
                }
                return CompList.Max(o => o.CompStatus);
            }
            set
            {
                WindTurbuineStatus = value;
            }*/
        }
        /// <summary>
        /// 组机状态时间
        /// </summary>
        public DateTime WindTurbuineStatusTime
        {
            get; set;
            /*get
            {
                if (CompList == null || CompList.Count == 0)
                {
                    return DateTime.MinValue;
                }
                return CompList.Max(o => o.CompStatusTime);
            }
            set
            {
                WindTurbuineStatusTime = value;
            }*/
        }

        /// <summary>
        /// 部件状态列表
        /// </summary>
        public List<DTCompStatus> CompList { get; set; }

    }
}

using ACH.DataEntity.Common;
using ACH.DataEntity.Enum;
using System;
using System.Collections.Generic;

namespace ACH.DataEntity.StatusTree
{
    /// <summary>
    /// 构建设备树 - 实体部件层 （DT表示DeviceTree，设备树）
    /// </summary>
    public class DTCompStatus
    {
        /// <summary>
        /// 部件Code
        /// </summary>
        public string CompCode { get; set; }

        /// <summary>
        /// 部件ID
        /// </summary>
        public string CompId { get; set; }

        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }

        /// <summary>
        /// 部件状态
        /// </summary>
        public EnumAlarmStatus CompStatus
        {
            get; set;
            /*get
            {
                if (MeaslocList == null || MeaslocList.Count == 0)
                {
                    return EnumAlarmStatus.Nostate;
                }
                return MeaslocList.Max(o => o.MeaslocStatus);
            }
            set
            {
                CompStatus = value;
            }*/
        }
        /// <summary>
        /// 部件状态时间
        /// </summary>
        public DateTime CompStatusTime
        {
            get; set;
            /*get
            {
                if (MeaslocList == null || MeaslocList.Count == 0)
                {
                    return DateTime.MinValue;
                }
                return MeaslocList.Max(o => o.MeaslocStatusTime);
            }
            set
            {
                CompStatusTime = value;
            }*/
        }

        /// <summary>
        /// 测点列表
        /// </summary>
        public List<DTMeaslocStatus> MeaslocList { get; set; }

        /// <summary>
        /// 诊断状态
        /// </summary>
        public Dictionary<DateTime, EnumDiagBaseType> CompDiagStatus { get; set; }
    }
}

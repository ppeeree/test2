using ACH.DataEntity.Common;
using SqlSugar;
using System;

namespace ACH.DataEntity.StatusTree
{
    /// <summary>
    /// 构建设备树 - 特征值层 （DT表示DeviceTree，设备树）
    /// </summary>
    public class DTEvStatus
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 测点ID
        /// </summary>
        public string MeasLocID { get; set; }

        /// <summary>
        /// 测量事件类型
        /// </summary>
        public EnumMonitorType? MeasType { get; set; }

        /// <summary>
        /// 部件类型
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string? CompType { get; set; }

        /// <summary>
        /// 部件ID
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string? CompID { get; set; }

        /// <summary>
        /// 特征值Code
        /// </summary>
        public string EvCode { get; set; }

        /// <summary>
        /// 特征值ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string EvId { get; set; }

        /// <summary>
        /// 特征值名称
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string EvName { get; set; }

        /// <summary>
        /// 特征值状态
        /// </summary>
        public EnumAlarmStatus EvStatus { get; set; }

        /// <summary>
        /// 特征值状态时间
        /// </summary>
        public DateTime EvStatusTime { get; set; }

        /// <summary>
        /// 信号类型名称
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string? SignalTypeName { get; set; }

        /// <summary>
        /// 信号类型Code
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string? SignalTypeCode { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string Unit { get; set; }

        /// <summary>
        /// 特征值数值
        /// </summary>
        public double Value { get; set; }


        public DTEvStatus() { }  // 无参构造方法
        public DTEvStatus(string deviceID, string measLocID, string evCode, string evId, string evName, EnumAlarmStatus evStatus, DateTime evStatusTime, string unit, double value, EnumMonitorType measType)// 有参构造方法
        {
            DeviceID = deviceID;
            EvCode = evCode;
            MeasLocID = measLocID;
            EvId = evId;
            EvName = evName;
            EvStatus = evStatus;
            EvStatusTime = evStatusTime;
            MeasType = measType;
            Unit = unit;
            Value = value;
        }
    }
}

using ACH.DataEntity.Common;
using ACH.DataEntity.Enum;
using System;
using System.Collections.Generic;

namespace ACH.DataEntity.DownLoad
{
    /// <summary>
    /// 数据下载传参
    /// </summary>
    public class DownloadParam
    {
        /// <summary>
        /// 风场名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 机组ID列表
        /// </summary>
        public List<string> DeviceIDs { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 每个机组的每个部件在时间段内的下载个数
        /// </summary>
        public int WaveNum { get; set; }
        /// <summary>
        /// 下载部件列表
        /// </summary>
        public List<EnumMonitorType> MeasTypes { get; set; }

        /// <summary>
        /// 数据保存格式
        /// </summary>
        public EnumDownloadWaveSaveType SaveType { get; set; }

        public DownloadParam(string StationName, List<string> deviceIDs, DateTime startTime, DateTime endTime, int waveNum, List<EnumMonitorType> measType, EnumDownloadWaveSaveType saveType)
        {
            // this.CreatedTime = createdTime;
            //this.StationIDs = stationIDs;
            this.StationName = StationName;
            this.DeviceIDs = deviceIDs;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.WaveNum = waveNum;
            this.MeasTypes = measType;
            this.SaveType = saveType;
        }

        public DownloadParam()
        {
            // 默认构造函数
        }
    }
}

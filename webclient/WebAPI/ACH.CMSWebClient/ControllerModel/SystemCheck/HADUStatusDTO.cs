using ACH.Alarm.Entity;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    /// <summary>
    /// HADU状态
    /// </summary>
    public class HADUStatusDTO
    {
        /// <summary>
        /// 表格唯一ID
        /// </summary>
        public string Guid { set; get; }

        /// <summary>
        /// 机组名称
        /// </summary>
        public string DeviceName { set; get; }

        /// <summary>
        /// 采集器ID
        /// </summary>
        public string MonitorID { set; get; }

        /// <summary>
        /// 采集器IP
        /// </summary>
        public string MonitorIP { set; get; }

        /// <summary>
        /// 采集器状态
        /// </summary>
        public string MonitorStatus { get; set; }

        /// <summary>
        /// 采集器状态时间
        /// </summary>
        public string MonitorStatusTime { get; set; }

        public HADUStatusDTO(string guid, string deviceName, string monitorId, string monitorIp, List<ChannelStatusAlarm> alarms)
        {
            Guid = guid;
            DeviceName = deviceName;
            MonitorID = monitorId;
            MonitorIP = monitorIp;
            MonitorStatus = alarms.Count > 0 && alarms.All(x => x.ChannelStatus == EnumChannelStatus.Normal) ? "正常" : "故障";
            MonitorStatusTime = alarms.Count > 0 ? alarms.Max(o => o.AcqTime).ToString("yyyy-MM-dd HH:mm:ss") : "--";
        }
    }
}

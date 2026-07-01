using ACH.Alarm.Entity;

namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    /// <summary>
    /// Modbus状态
    /// </summary>
    public class ModbusStatusDTO
    {
        /// <summary>
        /// 表格唯一ID
        /// </summary>
        public string Guid { set; get; }

        /// <summary>
        /// 采集器名称（倾角仪/晃度仪）
        /// </summary>
        public string MonitorName { set; get; }

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
        public string MonitorStatus { set; get; }

        /// <summary>
        /// 采集器状态时间
        /// </summary>
        public string MonitorStatusTime { set; get; }

        /// <summary>
        /// 采集器链接方式（串口通讯/TCP通讯）
        /// </summary>
        public string MonitorConnType { set; get; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="deviceName">机组名称</param>
        /// <param name="monitorId">采集器ID</param>
        /// <param name="monitorIp">采集器IP</param>
        /// <param name="monitorName">采集器名称（倾角仪/晃度仪）</param>
        /// <param name="monitorConnType">链接方式（串口/TCP）</param>
        /// <param name="data">数据</param>
        /// <param name="alarms">通道状态列表</param>
        public ModbusStatusDTO(string guid, string deviceName, string monitorId, string monitorIp, string monitorName, string monitorConnType, string data, List<ChannelStatusAlarm> alarms)
        {
            Guid = guid;
            DeviceName = deviceName;
            MonitorName = monitorName;
            MonitorID = monitorId;
            MonitorIP = monitorIp;
            MonitorStatus = alarms.Count > 0 && alarms.All(x => x.ChannelStatus == EnumChannelStatus.Normal) ? "正常" : "故障";
            MonitorStatusTime = alarms.Count > 0 ? alarms.Max(o => o.AcqTime).ToString("yyyy-MM-dd HH:mm:ss") : "--";
            MonitorConnType = monitorConnType;
            Data = data;
        }
    }

}

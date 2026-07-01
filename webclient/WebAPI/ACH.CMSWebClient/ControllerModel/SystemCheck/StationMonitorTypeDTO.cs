namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    /// <summary>
    /// 风场下采集器分组类型
    /// </summary>
    public class StationMonitorTypeDTO
    {
        /// <summary>
        /// 监测部件名称
        /// </summary>
        public string MonitorCompName { set; get; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public int Sort { set; get; }
        /// <summary>
        /// 监测部件类型列表
        /// </summary>
        public List<MonitorTypeDTO> MonitorTypeList { set; get; }
    }

    /// <summary>
    /// 采集器类型
    /// </summary>
    public class MonitorTypeDTO
    {
        /// <summary>
        /// 采集器类型名称
        /// </summary>
        public string MonitorTypeName { set; get; }
        /// <summary>
        /// 采集器类型Code   
        /// </summary>
        public string MonitorTypeCode { set; get; }
        /// <summary>
        /// 采集器类型异常个数
        /// </summary>
        public int FaultMonitorNum { set; get; }
    }
}

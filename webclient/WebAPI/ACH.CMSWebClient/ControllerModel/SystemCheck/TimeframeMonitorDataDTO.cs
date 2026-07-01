namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    /// <summary>
    /// 时间范围内采集器数据
    /// </summary>
    public class TimeframeMonitorDataDTO
    {
        /// <summary>
        /// 下限
        /// </summary>
        public double? LowerLine { get; set; } = 0.0;
        /// <summary>
        /// 上限
        /// </summary>
        public double? UpperLine { get; set; } = 0.0;
        /// <summary>
        /// 单位
        /// </summary>
        public string UnitY { set; get; }
        public string UnitX { set; get; } = "";
        /// <summary>
        /// 趋势数据
        /// </summary>
        public List<MonitorMeaslocDataDTO> Datas { get; set; }
    }

    public class MonitorMeaslocDataDTO
    {
        public string measlocName { get; set; }
        public List<object[]> data { get; set; }
    }
}

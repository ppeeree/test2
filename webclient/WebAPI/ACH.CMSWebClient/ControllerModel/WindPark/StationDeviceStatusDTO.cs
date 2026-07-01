namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationDeviceStatusDTO
    {
        /// <summary>
        /// 机组最新状态汇总
        /// </summary>
        public List<StationDayStatusDTO>? DayStatusList { get; set; }

        /// <summary>
        /// 机组周状态汇总
        /// </summary>
        public List<StationWeekStatusDTO>? WeekStatusList { get; set; }
    }

    /// <summary>
    /// 风场机组的天状态
    /// </summary>
    public class StationDayStatusDTO
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        public string? StatusName { get; set; }
        /// <summary>
        /// 状态Code
        /// </summary>
        public string? StatusCode { get; set; }
        /// <summary>
        /// 状态数量
        /// </summary>
        public int DeviceCount { get; set; }
    }

    /// <summary>
    /// 风场机组的周状态
    /// </summary>
    public class StationWeekStatusDTO
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string? StatusTime { get; set; }
        /// <summary>
        /// 该天的状态数据 ["normal", "正常", 2]
        /// </summary>
        public List<List<Object>> Data { get; set; }
    }
}

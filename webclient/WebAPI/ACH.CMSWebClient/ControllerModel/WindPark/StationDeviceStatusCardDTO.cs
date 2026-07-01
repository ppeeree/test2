namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationDeviceStatusCardDTO
    {
        /// <summary>
        /// 采集器状态
        /// </summary>
        public string? DauStatus { get; set; } = "normal";
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// 风场ID
        /// </summary>
        public string? WindparkId { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string? WindturbineId { get; set; }
        /// <summary>
        /// 机组名称
        /// </summary>
        public string? WindturbineName { get; set; }
        /// <summary>
        /// 机组状态
        /// </summary>
        public string? WindturbineStatus { get; set; }
        /// <summary>
        /// 状态时间
        /// </summary>
        public string? StatusLastTime { get; set; }
        /// <summary>
        /// 部件状态信息列表
        /// </summary>
        public List<StationDeviceCardCompStatusDTO> HealthStatusEntityVo { get; set; }
    }

    public class StationDeviceCardCompStatusDTO
    {
        /// <summary>
        /// 部件ID
        /// </summary>
        public string? EntityId { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string? EntityName { get; set; }
        /// <summary>
        /// 部件状态
        /// </summary>
        public string? EntityStatus { get; set; }
        /// <summary>
        /// 状态时间
        /// </summary>
        public string? StatusTime { get; set; }
        /// <summary>
        /// 部件类型
        /// </summary>
        public string? EntityType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;
    }
}

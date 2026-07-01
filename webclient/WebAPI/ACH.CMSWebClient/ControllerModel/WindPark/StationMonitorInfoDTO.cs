namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationMonitorInfoDTO
    {
        /// <summary>
        /// 风场Code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 风场Id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 风场名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 监控机组个数
        /// </summary>
        public int MonitorNumber { get; set; }

        /// <summary>
        /// 监控机组Id
        /// </summary>
        public List<string>? MonitorNumberIds { get; set; }

        /// <summary>
        /// 监控率
        /// </summary>
        public double MonitoringRate { get; set; }

        /// <summary>
        /// 离线机组个数
        /// </summary>
        public int OfflineNumber { get; set; }

        /// <summary>
        /// 离线机组Id
        /// </summary>
        public List<string?> OfflineNumberIds { get; set; }
    }
}

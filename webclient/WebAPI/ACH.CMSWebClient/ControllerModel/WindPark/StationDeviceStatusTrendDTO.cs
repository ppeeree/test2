namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationDeviceStatusTrendDTO
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
        /// 状态数据 [["2026-01-01",10]]
        /// </summary>
        public List<List<Object>> Data { get; set; }
    }
}

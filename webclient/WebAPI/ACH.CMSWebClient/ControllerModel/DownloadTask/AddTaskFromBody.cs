namespace ACH.CMSWebClient.ControllerModel.DownloadTask
{
    /// <summary>
    /// 新增下载任务POST接口传参
    /// </summary>
    public class AddTaskFromBody
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createdTime { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string stationName { get; set; }
        /// <summary>
        /// 机组ID列表
        /// </summary>
        public string[] windturbineIDs { get; set; }
        /// <summary>
        /// 筛选时间范围内波形：开始时间
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 筛选时间范围内波形：结束时间
        /// </summary>
        public string endTime { get; set; }
        /// <summary>
        /// 筛选波形个数
        /// </summary>
        public string waveNum { get; set; }
        /// <summary>
        /// 筛选波形类型
        /// </summary>
        public string[] measType { get; set; }

        /// <summary>
        /// 波形保存类型
        /// </summary>
        public string WaveSaveType { get; set; }
    }
}

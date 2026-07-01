namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 查询特征值趋势
    /// </summary>
    public class EvTrendFromBodyDTO
    {
        /// <summary>
        /// 诊断分析趋势方式
        /// </summary>
        public string analyzeWay { get; set; }
        /// <summary>
        /// 特征值ID列表
        /// </summary>
        public List<string>? eigenValueIds { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string endTime { get; set; }
        /// <summary>
        /// 工况范围
        /// </summary>
        public Dictionary<string, List<double?>> wkCond { get; set; }
        /// <summary>
        /// 机组ID列表
        /// </summary>
        public List<string>? windturbineIds { get; set; }
    }
}

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 风场诊断报告汇总信息DTO
    /// </summary>
    [Serializable]
    public class WindParkDiagReportSummaryDTO : WindParkInfoDTO
    {
        /// <summary>
        /// 已诊断机组数量
        /// </summary>
        public int DiagnosisWindturbineCount { get; set; }
        /// <summary>
        /// 未诊断机组数量
        /// </summary>
        public int NoDiagnosisWindturbineCount { get; set; }
        /// <summary>
        /// 子节点信息，风电场下所有设备的信息
        /// </summary>
        public List<WindParkDiagReportSummaryLine> Children { get; set; }
    }
    /// <summary>
    /// 风场诊断报告详情信息DTO,继承汇总信息DTO,增加报告月份等信息
    /// </summary>
    [Serializable]
    public class WindParkDiagReportDetailDTO : WindParkDiagReportSummaryDTO
    {
        /// <summary>
        /// 报告月份
        /// </summary>
        public DateTime ReportMonth { get; set; }
        /// <summary>
        /// 监测时间开始
        /// </summary>
        public DateTime MonitoringTimeStart { get; set; }
        /// <summary>
        /// 监测时间结束
        /// </summary>
        public DateTime MonitoringTimeEnd { get; set; }
        /// <summary>
        /// 项目概述
        /// </summary>
        public string ProjectOvewerview { get; set; }

        public DateTime CreateTime { get; set; }

        public string ReportName { get; set; }
    }

}

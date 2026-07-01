namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 风电场诊断报告DTO，用于前端列表展示。
    /// </summary>
    public class WindParkDiagReportDTO
    {
        /// <summary>
        /// 报告唯一标识
        /// </summary>
        public string ReportGuid { get; set; }
        /// <summary>
        /// 风场唯一标识
        /// </summary>
        public string WindParkId { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindParkName { get; set; }
        /// <summary>
        /// 报告名称
        /// </summary>
        public string ReportName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }

}

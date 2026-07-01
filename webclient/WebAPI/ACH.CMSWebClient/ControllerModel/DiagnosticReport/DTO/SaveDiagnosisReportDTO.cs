namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 诊断报告保存DTO
    /// </summary>
    public class SaveDiagnosisReportDTO
    {
        /// <summary>
        /// 机组ID - 机组信息由机组ID从其他表中带出
        /// </summary>
        public string WindturbineId { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        public string RuningAdvice { get; set; }

        /// <summary>
        /// 机组状态
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// 样本数据速度
        /// </summary>
        public double? SampleDataSpeed { get; set; }
        /// <summary>
        /// 报告时间
        /// </summary>
        public DateTime? ReportTime { get; set; }

        /// <summary>
        /// 机组分析记录详情DTO列表
        /// </summary>
        public List<SaveSimpleAnalyzerRecordDTO> AnalyzerRecords { get; set; }
        /// <summary>
        /// 诊断报告结论详情DTO列表
        /// </summary>
        public List<DiagnosisReportConclusionDTO> Conclusions { get; set; }
    }
    /// <summary>
    /// 简单分析记录保存DTO
    /// </summary>
    [Serializable]
    public class SaveSimpleAnalyzerRecordDTO
    {
        public int AnalyzerRecordId { get; set; }
        public string Description { get; set; }
    }

}

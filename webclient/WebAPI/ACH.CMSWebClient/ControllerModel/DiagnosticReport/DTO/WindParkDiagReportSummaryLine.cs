using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 风场诊断报告汇总明细行
    /// </summary>
    public class WindParkDiagReportSummaryLine : ISortable
    {
        /// <summary>
        /// 诊断报告Guid
        /// </summary>
        public string WindturbingReportGuid { get; set; }
        /// <summary>
        /// 单条记录的Guid
        /// </summary>
        public string DiagRecordGuid { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindturbineId { get; set; }
        /// <summary>
        /// 是否诊断
        /// </summary>
        public bool IsDiagnosis { get; set; }
        /// <summary>
        /// 机组名称
        /// </summary>
        public string WindturbineName { get; set; }
        /// <summary>
        /// 诊断时间
        /// </summary>
        public string DiagnosisTime { get; set; }
        /// <summary>
        /// 健康状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }
        /// <summary>
        /// 部件健康状态
        /// </summary>
        public string CompStatus { get; set; }
        /// <summary>
        /// 诊断结论
        /// </summary>
        public string DiagnosisConclusion { get; set; }
        /// <summary>
        /// 维护建议
        /// </summary>
        public string MaintainAdvice { get; set; }
        /// <summary>
        /// 运行建议
        /// </summary>
        public string RuningAdvice { get; set; }
        /// <summary>
        /// 诊断结论状态
        /// </summary>
        public string DiagnosisConclusionStatus { get; set; }

        public string GetSortableName() => CompName ?? string.Empty;
    }

}

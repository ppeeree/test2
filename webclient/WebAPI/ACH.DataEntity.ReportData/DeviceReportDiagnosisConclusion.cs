using SqlSugar;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 报告诊断结论
    /// </summary>
    [SugarTable("devicereportdiagnosisconclusion")]
    public class DeviceReportDiagnosisConclusion
    {
        /// <summary>
        /// 主键 - 32位UUID，用于关联该报告的诊断结论以及分析记录
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "reportGuid")]
        public string ReportGuid { get; set; }

        /// <summary>
        /// 主键 - 联合主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "lineId")]
        public int LineId { get; set; }

        /// <summary>
        /// 部件名称
        /// </summary>
        [SugarColumn(ColumnName = "compName")]
        public string CompName { get; set; }

        /// <summary>
        /// 诊断结论
        /// </summary>
        [SugarColumn(ColumnName = "diagnosisConclusion")]
        public string DiagnosisConclusion { get; set; }

        /// <summary>
        /// 预警等级
        /// </summary>
        [SugarColumn(ColumnName = "warningLevel")]
        public string WarningLevel { get; set; }

        /// <summary>
        /// 维护建议
        /// </summary>
        [SugarColumn(ColumnName = "maintainAdvice")]
        public string MaintainAdvice { get; set; }
    }
}

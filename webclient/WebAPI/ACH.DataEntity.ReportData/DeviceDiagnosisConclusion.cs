using SqlSugar;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 诊断结论表 -- 机组诊断结论
    /// </summary>
    [SugarTable("devicediagnosisconclusion")]
    [SugarIndex("idx_windturbineId", nameof(WindturbineId), OrderByType.Asc)]
    public class DeviceDiagnosisConclusion
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// 机组ID
        /// </summary>
        [SugarColumn(ColumnName = "windturbineId", IsNullable = false)]
        public string WindturbineId { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        [SugarColumn(ColumnName = "compName", IsNullable = false)]
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

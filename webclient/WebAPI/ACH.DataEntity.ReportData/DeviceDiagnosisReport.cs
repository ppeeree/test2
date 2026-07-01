using SqlSugar;
using System;

namespace ACH.DataEntity.ReportData
{
    // <summary>
    /// 机组诊断报告
    /// </summary>
    [SugarTable("devicediagnosisreport")]
    [SugarIndex("idx_windturbineId", nameof(WindturbineId), OrderByType.Asc)]
    public class DeviceDiagnosisReport
    {
        /// <summary>
        /// 主键 - 32位UUID，用于关联该报告的诊断结论以及分析记录
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "reportGuid")]
        public string ReportGuid { get; set; }

        /// <summary>
        /// 机组ID - 机组信息由机组ID从其他表中带出
        /// </summary>
        [SugarColumn(ColumnName = "windturbineId", IsNullable = false)]
        public string WindturbineId { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        [SugarColumn(ColumnName = "runingAdvice")]
        public string RuningAdvice { get; set; }

        /// <summary>
        /// 机组状态
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public string Status { get; set; }
        /// <summary>
        /// 样本数据发电机转速
        /// </summary>
        [SugarColumn(ColumnName = "sampleDataSpeed")]
        public double? SampleDataSpeed { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "createdTime")]
        public DateTime CreatedTime { get; set; }
    }
}

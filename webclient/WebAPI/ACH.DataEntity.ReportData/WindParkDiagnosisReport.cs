using SqlSugar;
using System;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 风场诊断报告实体类
    /// </summary>
    [SugarTable("windparkdiagnosisreport")]
    [SugarIndex("idx_windParkId", nameof(WindParkId), OrderByType.Asc)]
    public class WindParkDiagnosisReport
    {
        /// <summary>
        /// 报告唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "reportGuid")]
        public string ReportGuid { get; set; }
        /// <summary>
        /// 风场唯一标识
        /// </summary>
        [SugarColumn(ColumnName = "windParkId")]
        public string WindParkId { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        [SugarColumn(ColumnName = "windParkName")]
        public string WindParkName { get; set; }
        /// <summary>
        /// 报告月份
        /// </summary>
        [SugarColumn(ColumnName = "reportMonth")]
        public DateTime ReportMonth { get; set; }
        /// <summary>
        /// 报告名称
        /// </summary>
        [SugarColumn(ColumnName = "reportName")]
        public string ReportName { get; set; }
        /// <summary>
        /// 监测时间开始和结束
        /// </summary>
        [SugarColumn(ColumnName = "monitoringTimeStart")]
        public DateTime MonitoringTimeStart { get; set; }
        /// <summary>
        /// 监测时间开始和结束
        /// </summary>
        [SugarColumn(ColumnName = "monitoringTimeEnd")]
        public DateTime MonitoringTimeEnd { get; set; }
        /// <summary>
        /// 项目概述
        /// </summary>
        [SugarColumn(ColumnName = "projectOverview")]
        public string ProjectOverview { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "createTime")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        [SugarColumn(ColumnName = "remark")]
        public string Remark { get; set; }

    }
}

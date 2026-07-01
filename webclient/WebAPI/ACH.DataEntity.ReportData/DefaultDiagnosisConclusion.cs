using SqlSugar;
using System;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 默认诊断结论表 -- 此表内容不会被页面操作更新
    /// </summary>
    [SugarTable("DefaultDiagnosisConclusion")]
    public class DefaultDiagnosisConclusion
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 部件名称
        /// </summary>
        //[SugarColumn(ColumnName = "compName")]
        public string CompName { get; set; }

        /// <summary>
        /// 测量位置名称
        /// </summary>
        //[SugarColumn(ColumnName = "measlocName")]
        public string MeaslocName { get; set; }

        /// <summary>
        /// 诊断结论
        /// </summary>
        //[SugarColumn(ColumnName = "diagnosisConclusion")]
        public string DiagnosisConclusion { get; set; }

        /// <summary>
        /// 预警等级
        /// </summary>
        //[SugarColumn(ColumnName = "warningLevel")]
        public string WarningLevel { get; set; }

        /// <summary>
        /// 维护建议
        /// </summary>
        //[SugarColumn(ColumnName = "maintainAdvice")]
        public string MaintainAdvice { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        //[SugarColumn(ColumnName = "createdTime")]
        public DateTime CreatedTime { get; set; }
    }
}

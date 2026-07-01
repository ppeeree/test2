using SqlSugar;
using System;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 默认运行建议表 -- 此表内容不会被页面操作更新
    /// </summary>
    [SugarTable("defaultruningadvice")]
    public class DefaultRuningAdvice
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// 预警等级
        /// </summary>
        [SugarColumn(ColumnName = "warningLevel")]
        public string WarningLevel { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        [SugarColumn(ColumnName = "runingAdvice")]
        public string RuningAdvice { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "createdTime")]
        public DateTime CreatedTime { get; set; }
    }
}

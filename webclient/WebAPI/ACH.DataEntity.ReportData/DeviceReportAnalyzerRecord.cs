using ACH.DataEntity.Enum;
using ACH.Helper.ImageSizeReader;
using SqlSugar;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 报告分析记录
    /// </summary>
    [SugarTable("devicereportanalyzerrecord")]
    public class DeviceReportAnalyzerRecord
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
        /// 测量位置名称
        /// </summary>
        [SugarColumn(ColumnName = "measlocId")]
        public string MeaslocId { get; set; }
        /// <summary>
        /// 测量位置名称
        /// </summary>

        [SugarColumn(ColumnName = "measlocName")]
        public string MeaslocName { get; set; }

        /// <summary>
        /// 特征值类型
        /// </summary>
        [SugarColumn(ColumnName = "eigenvalueId")]
        public string EigenValueId { get; set; }

        /// <summary>
        /// 图谱类型
        /// </summary>
        [SugarColumn(ColumnName = "imageType")]
        public string ImageType { get; set; }

        /// <summary>
        /// 分析图谱
        /// </summary>
        [SugarColumn(ColumnName = "image")]
        public byte[] Image { get; set; }

        /// <summary>
        /// 图片类型
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public EnumImageType ImageUrlType { get; set; }

        /// <summary>
        /// 分析描述
        /// </summary>
        [SugarColumn(ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// 机组分析记录主键ID
        /// </summary>
        [SugarColumn(ColumnName = "analyzerRecordId")]
        public int AnalyzerRecordId { get; set; }
    }
}

using ACH.DataEntity.Enum;
using ACH.Helper.ImageSizeReader;
using SqlSugar;
using System;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 机组分析记录表 
    /// 针对趋势图 -- 同部件同测量位置同特征值类型只保留一条分析描述信息
    /// 针对时域波形或频谱波形 -- 同部件同测量位置只保留一条分析描述信息
    /// </summary>
    [SugarTable("devicediagnosisanalyzerrecord")]
    [SugarIndex("idx_windturbineId", nameof(WindturbineId), OrderByType.Asc)]
    public class DeviceDiagnosisAnalyzerRecord
    {
        /// <summary>
        /// 自增ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// 机组ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "windturbineId")]
        public string WindturbineId { get; set; }

        /// <summary>
        /// 部件名称
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "compName")]
        public string CompName { get; set; }

        /// <summary>
        /// 测量位置名称
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "measlocId")]
        public string MeaslocId { get; set; }
        /// <summary>
        /// 测量位置名称
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "measlocName")]
        public string MeaslocName { get; set; }

        /// <summary>
        /// 特征值类型
        /// </summary>
        [SugarColumn(ColumnName = "eigenValueId")]
        public string EigenValueId { get; set; }

        /// <summary>
        /// 图谱类型
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "imageType")]
        public string ImageType { get; set; }

        /// <summary>
        /// 分析图谱
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "image")]
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
        /// 采集时间
        /// </summary>
        [SugarColumn(ColumnName = "acqTime", IsNullable = true)]
        public DateTime? AcqTime { get; set; }
        /// <summary>
        /// 平均值
        /// </summary>
        [SugarColumn(ColumnName = "avg", IsNullable = true)]
        public double? Avg { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "recordTime")]
        public DateTime RecordTime { get; set; }
    }
}

using SqlSugar;

namespace ACH.DataEntity.ReportData
{
    /// <summary>
    /// 风场报告与风场设备关系表
    /// </summary>
    [SugarTable("windparkreportdevicerelation")]
    public class WindParkReportDeviceRelation
    {
        /// <summary>
        /// 风场报告ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "windParkReportGuid")]
        public string WindParkReportGuid { get; set; }
        /// <summary>
        /// 风场设备报告ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "windTurbineReportGuid")]
        public string WindTurbineReportGuid { get; set; }
        /// <summary>
        /// 风场设备ID
        /// </summary>
        [SugarColumn(ColumnName = "windTurbineId")]
        public string WindTurbineId { get; set; }
        /// <summary>
        /// 风场设备名称
        /// </summary>
        [SugarColumn(ColumnName = "windTurbineName")]
        public string WindTurbineName { get; set; }
    }
}

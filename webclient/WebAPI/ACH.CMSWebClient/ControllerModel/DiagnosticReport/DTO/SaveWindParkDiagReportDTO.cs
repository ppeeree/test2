namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 保存风电场诊断报告的DTO。
    /// </summary>
    public class SaveWindParkDiagReportDTO
    {
        /// <summary>
        /// 风电场ID。
        /// </summary>
        public string WindParkId { get; set; }
        /// <summary>
        /// 风电场名称。
        /// </summary>
        public string? WindParkName { get; set; }
        /// <summary>
        /// 报告月份。
        /// </summary>
        public DateTime ReportMonth { get; set; }
        /// <summary>
        /// 监测时间开始。
        /// </summary>
        public DateTime MonitoringTimeStart { get; set; }
        /// <summary>
        /// 监测时间结束。
        /// </summary>
        public DateTime MonitoringTimeEnd { get; set; }
        /// <summary>
        /// 报告名称。
        /// </summary>
        public string ReportName { get; set; }
        /// <summary>
        /// 备注。
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 项目概述。
        /// </summary>
        public string ProjectOverview { get; set; }
        /// <summary>
        /// 风电场设备关系。
        /// </summary>
        public List<SaveWindParkDeviceRelationDTO> WindParkDeviceRelations { get; set; } = new List<SaveWindParkDeviceRelationDTO>();
    }

    public class SaveWindParkDeviceRelationDTO
    {
        public string WindTurbineReportGuid { get; set; }

        public string WindTurbineId { get; set; }

        public string? WindTurbineName { get; set; }
    }


}

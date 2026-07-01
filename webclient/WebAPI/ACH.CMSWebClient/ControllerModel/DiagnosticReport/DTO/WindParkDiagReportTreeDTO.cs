namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    ///  风场诊断报告树结构DTO
    /// </summary>
    [Serializable]
    public class SimpleWindParkDiagReportDTO
    {
        public string Type { get { return "WindParkReport"; } }

        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }
        public List<WindParkDeviceReportRelDTO> Children { get; set; }
    }
    [Serializable]
    public class WindParkDeviceReportRelDTO
    {
        public string Type { get { return "WindTurbineReport"; } }
        public string Id { get; set; }
        public string Status { get; set; }
        public string WindturbineId { get; set; }
        public string name { get; set; }
        public string CreateTime { get; set; }
    }
}

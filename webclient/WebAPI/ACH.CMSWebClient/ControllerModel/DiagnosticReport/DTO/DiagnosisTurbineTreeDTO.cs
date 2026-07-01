namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 诊断报告树形结构
    /// </summary>
    [Serializable]
    public class DiagnosisTurbineTreeDTO
    {
        public DiagnosisTurbineTreeDTO()
        {
            Children = new List<TurbineDTO>();
        }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 风场ID
        /// </summary>
        public string Id { get; set; }

        public string Type { get { return "WindPark"; } }
        /// <summary>
        /// 风场下的所有风机信息
        /// </summary>
        public List<TurbineDTO> Children { get; set; }
    }

    [Serializable]
    public class TurbineDTO
    {
        public string Type { get { return "Windturbine"; } }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindParkName { get; set; }
        /// <summary>
        /// 状态信息，例如：正常、故障等
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 风机编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 风机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 主机厂家
        /// </summary>
        public string Manufactory { get; set; }
        /// <summary>
        /// 风机型号
        /// </summary>
        public string WindTurbineType { get; set; }
        /// <summary>
        /// 传动形式及传动比
        /// </summary>
        public string TransmissionFormAndRatio { get; set; }
        /// <summary>
        /// 样本数据发电机转速
        /// </summary>
        public double SampleDataSpeed { get; set; }
        /// <summary>
        /// 发电机额定转速
        /// </summary>
        public double RatedGeneratorSpeed { get; set; }
        /// <summary>
        /// 诊断报告列表
        /// </summary>
        public List<WindturbineReportDTO> Children { get; set; }
    }
    [Serializable]
    public class WindturbineReportDTO
    {
        public string Id { get; set; }
        public string Type { get { return "WindturbineReport"; } }

        /// <summary>
        /// 机组ID - 机组信息由机组ID从其他表中带出
        /// </summary>
        public string WindturbineId { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        public string RuningAdvice { get; set; }

        /// <summary>
        /// 机组状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedTime { get; set; }
        /// <summary>
        /// 诊断报告是否关联风场
        /// </summary>
        public bool IsCorrelationWindpark { get; set; }
    }

}

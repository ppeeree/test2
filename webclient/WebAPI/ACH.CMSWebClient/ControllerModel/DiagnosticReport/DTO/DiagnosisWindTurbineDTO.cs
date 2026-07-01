namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 诊断设备信息
    /// </summary>
    [Serializable]
    public class DiagnosisWindTurbineDTO
    {
        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindParkName { get; set; }
        /// <summary>
        /// 风机编号
        /// </summary>
        public string WindTurbineID { get; set; }
        /// <summary>
        /// 风机名称
        /// </summary>
        public string WindTurbineName { get; set; }
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
    }

}

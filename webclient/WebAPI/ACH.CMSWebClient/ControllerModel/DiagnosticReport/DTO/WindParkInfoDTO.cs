namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 风电场信息DTO
    /// </summary>
    [Serializable]
    public class WindParkInfoDTO
    {
        /// <summary>
        /// 风电场ID
        /// </summary>
        public string Id { get; set; }
        public string Type { get { return "WindPark"; } }
        /// <summary>
        /// 风电场名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 风电场地址 => 查询DevOrganization表获取风电场地址信息，ID为WindParkId
        /// </summary>
        public string WindParkAddress { get; set; }
        /// <summary>
        /// 风机数量
        /// </summary>
        public int WindTurbineCount { get; set; }
        /// <summary>
        /// 风机型号，从该风场的所有风机中随机选取一个机型即可
        /// </summary>
        public string WindTurbineType { get; set; }
        /// <summary>
        /// 投运时间，从该风场的所有风机中随机选取一个投运时间即可
        /// </summary>
        public string OperationlDate { get; set; }
        /// <summary>
        /// 传动形式，从该风场的所有风机中随机选取一个传动形式即可
        /// </summary>
        public string TransmissionForm { get; set; }
        /// <summary>
        /// 检测对象，传动链风场均为“传动链”，固定值
        /// </summary>
        public string DetectionObject { get; set; }
        /// <summary>
        /// 检测设备，固定信息“众芯汉创 WTPHM.BD-TF-DT”
        /// </summary>
        public string DetectionDevice { get; set; }
        /// <summary>
        /// 检测方法，固定信息“在线振动监测”
        /// </summary>
        public string DetectionMethod { get; set; }
    }

}

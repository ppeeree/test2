namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 特征值趋势
    /// </summary>
    public class EvTrendDTO
    {
        /// <summary>
        /// 平均值
        /// </summary>
        public double Avg { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// 中位数
        /// </summary>
        public double Mid { get; set; }
        /// <summary>
        /// X轴单位
        /// </summary>
        public string UnitX { get; set; }
        /// <summary>
        /// Y轴单位
        /// </summary>
        public string UnitY { get; set; }
        /// <summary>
        /// VDI最大值
        /// </summary>
        public double? VdiMax { get; set; }
        /// <summary>
        /// VDI最小值
        /// </summary>
        public double? VdiMin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<EvTrendItemDTO> EvdataList { get; set; }
    }

    public class EvTrendItemDTO
    {
        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindParkName { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindturbineId { get; set; }
        /// <summary>
        /// 机组名称
        /// </summary>
        public string WindturbineName { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MeasdefId { get; set; }
        /// <summary>
        /// 测点ID
        /// </summary>
        public string MeaslocId { get; set; }
        /// <summary>
        /// 测点名称
        /// </summary>
        public string MeaslocName { get; set; }
        /// <summary>
        /// 测点Code
        /// </summary>
        public string MeaslocCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WaveDefId { get; set; }
        /// <summary>
        /// 特征值Code
        /// </summary>
        public string EvCode { get; set; }
        /// <summary>
        /// 特征值ID
        /// </summary>
        public string EvId { get; set; }
        /// <summary>
        /// 特征值名称
        /// </summary>
        public string EvName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BandWidth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DisVdiMax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DisVdiMin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DisVdiMaxNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DisVdiMinNum { get; set; }
        /// <summary>
        /// 平均值
        /// </summary>
        public double Avg { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// 中位数
        /// </summary>
        public double Mid { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// 采样频率
        /// </summary>
        public string SampleRate { get; set; }
        /// <summary>
        /// 采样时长
        /// </summary>
        public string Samplingtimelength { get; set; }
        /// <summary>
        /// X轴单位
        /// </summary>
        public string UnitX { get; set; }
        /// <summary>
        /// Y轴单位
        /// </summary>
        public string UnitY { get; set; }
        /// <summary>
        /// VDI最大值
        /// </summary>
        public double? VdiMax { get; set; }
        /// <summary>
        /// VDI最小值
        /// </summary>
        public double? VdiMin { get; set; }
        /// <summary>
        /// 注意值
        /// </summary>
        public double? Attention { get; set; }
        /// <summary>
        /// 警告值
        /// </summary>
        public double? Warning { get; set; }
        /// <summary>
        /// 危险值
        /// </summary>
        public double? Danger { get; set; }

        /// <summary>
        /// 特征值数值
        /// </summary>
        public List<List<object>> Evdata { get; set; }
    }
}

namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    public class BFMEchoWaveDTO
    {
        /// <summary>
        /// 单位
        /// </summary>
        public string UnitX { set; get; }
        public string UnitY { set; get; }
        /// <summary>
        /// 趋势数据
        /// </summary>
        public List<BFMEchoWaveDataDTO> Datas { get; set; }
    }

    public class BFMEchoWaveDataDTO
    {
        public string Legend { get; set; }
        public List<List<double>> data { get; set; }
    }
}

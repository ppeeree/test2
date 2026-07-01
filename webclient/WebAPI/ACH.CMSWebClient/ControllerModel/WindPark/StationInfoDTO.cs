namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationInfoDTO
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Code { get; set; } = "windpark";
        /// <summary>
        /// 风场ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 风场机组数量
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 厂商
        /// </summary>
        public List<StationInfoDetailDTO> childNode { get; set; }
        /// <summary>
        /// 投运时间
        /// </summary>
        public List<StationInfoDetailDTO> childNode1 { get; set; }
        /// <summary>
        /// 风机型号
        /// </summary>
        public List<StationInfoDetailDTO> childNode2 { get; set; }
    }

    public class StationInfoDetailDTO
    {
        /// <summary>
        /// 风机ID列表
        /// </summary>
        public List<string> Ids { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 比例
        /// </summary>
        public double Ratio { set; get; }
        /// <summary>
        /// 该风场ID列表个数
        /// </summary>
        public int Size { set; get; }
    }
}

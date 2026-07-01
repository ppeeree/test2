namespace ACH.CMSWebClient.ControllerModel.Config
{
    /// <summary>
    /// 针对EigenValueConfigMap.json的解析类
    /// </summary>
    public class EvConfigMapperJsonDTO
    {
        /// <summary>
        /// 聚合部件名称
        /// </summary>
        public string compType { get; set; }
        /// <summary>
        /// 实体部件列表
        /// </summary>
        public List<compItem> compList { get; set; }
    }

    public class compItem
    {
        public string compType { get; set; }
        public List<compValueItem> compList { get; set; }
    }

    public class compValueItem
    {
        public string name { get; set; }
        public string code { get; set; }
        public bool isFirst { get; set; }
    }
}

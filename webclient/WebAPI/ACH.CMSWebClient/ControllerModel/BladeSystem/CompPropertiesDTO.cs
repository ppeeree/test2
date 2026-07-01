namespace ACH.CMSWebClient.ControllerModel.BladeSystem
{
    /// <summary>
    /// 部件属性
    /// </summary>
    public class CompPropertiesDTO
    {
        /// <summary>
        /// 部件名称
        /// </summary>
        public string entityName { get; set; }
        /// <summary>
        /// 部件ID
        /// </summary>
        public string entityId { get; set; }
        /// <summary>
        /// 部件属性详细信息
        /// </summary>
        public List<CompPropertiesItemDTO> entityInfo { get; set; }
    }

    public class CompPropertiesItemDTO
    {
        public string key { get; set; }
        public string name { get; set; }
        public string[] pos { get; set; }
        public string type { get; set; }
        public string unit { get; set; }
        public string value { get; set; }
    }
}

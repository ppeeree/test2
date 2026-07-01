namespace ACH.CMSWebClient.ControllerModel.BladeSystem
{
    /// <summary>
    /// 系统按钮模型
    /// </summary>
    public class SystemButtonDTO
    {
        public string id { get; set; }
        public string parentId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string alias { get; set; }
        public string path { get; set; }
        public string source { get; set; }
        public int sort { get; set; }
        public int category { get; set; }
        public int action { get; set; }
        public int isOpen { get; set; }
        public string remark { get; set; }
        public int isDeleted { get; set; }
        public List<SystemButtonChildrenDTO> children { get; set; }
        public bool hasChildren { get; set; }
        public string parentName { get; set; }
        public string categoryName { get; set; }
        public string actionName { get; set; }
        public string isOpenName { get; set; }
    }

    /// <summary>
    /// 系统按钮子项模型
    /// </summary>
    public class SystemButtonChildrenDTO
    {
        public string id { get; set; }
        public string parentId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string alias { get; set; }
        public string path { get; set; }
        public string source { get; set; }
        public int sort { get; set; }
        public int category { get; set; }
        public int action { get; set; }
        public int isOpen { get; set; }
        public string remark { get; set; }
        public int isDeleted { get; set; }
        public bool hasChildren { get; set; }
        public string parentName { get; set; }
        public string categoryName { get; set; }
        public string actionName { get; set; }
        public string isOpenName { get; set; }
    }
}

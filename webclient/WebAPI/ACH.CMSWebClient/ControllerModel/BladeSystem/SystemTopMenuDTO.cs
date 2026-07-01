namespace ACH.CMSWebClient.ControllerModel.BladeSystem
{
    public class SystemTopMenuDTO
    {
        public string code { get; set; }
        public string createDept { get; set; }
        public string createTime { get; set; }
        public string createUser { get; set; }
        public string id { get; set; }
        public int isDeleted { get; set; }
        public string name { get; set; }
        public int sort { get; set; }
        public string source { get; set; }
        public int status { get; set; }
        public string tenantId { get; set; }
        public string updateTime { get; set; }
        public string updateUser { get; set; }
        public List<SystemSecondMenuDTO> children { get; set; }
    }

    public class SystemSecondMenuDTO
    {
        public int action { get; set; }
        public string actionName { get; set; }
        public string alias { get; set; }
        public int category { get; set; }
        public string categoryName { get; set; }
        public string code { get; set; }
        public bool hasChildren { get; set; }
        public string id { get; set; }
        public int isDeleted { get; set; }
        public int isOpen { get; set; }
        public string isOpenName { get; set; }
        public string name { get; set; }
        public string parentId { get; set; }
        public string parentName { get; set; }
        public string path { get; set; }
        public string remark { get; set; }
        public int sort { get; set; }
        public string source { get; set; }
    }
}

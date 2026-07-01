namespace ACH.CMSWebClient.ControllerModel.Component
{
    public class DeviceEvCardStatusDTO
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string? TurbineId { get; set; }
        /// <summary>
        /// 机组状态
        /// </summary>
        public string? TurbineState { get; set; }
        /// <summary>
        /// 机组状态时间
        /// </summary>
        public string? TurbineStateTime { get; set; }
        /// <summary>
        /// 聚合部件列表
        /// </summary>
        public List<PageCompItemModel> PagecompList { get; set; }
    }

    public class PageCompItemModel
    {
        /// <summary>
        /// 聚合部件Code
        /// </summary>
        public string? PagecompCode { get; set; }
        /// <summary>
        /// 聚合部件名称
        /// </summary>
        public string? PagecompName { get; set; }
        /// <summary>
        /// 实体部件列表
        /// </summary>
        public List<CompItemModel> CompList { get; set; }

        // 构造函数
        public PageCompItemModel(string pagecompCode, string pagecompName)
        {
            PagecompCode = pagecompCode;
            PagecompName = pagecompName;
            CompList = new List<CompItemModel>();
        }

        // 添加部件方法
        public void AddCompItem(CompItemModel item)
        {
            CompList.Add(item);
        }
    }


    public class CompItemModel
    {
        /// <summary>
        /// 部件Code
        /// </summary>
        public string? CompCode { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string? CompName { get; set; }
        /// <summary>
        /// 部件ID
        /// </summary>
        public string? CompId { get; set; }
        /// <summary>
        /// 部件状态
        /// </summary>
        public string? CompState { get; set; }
        /// <summary>
        /// 部件状态时间
        /// </summary>
        public string? CompStateTime { get; set; }
        /// <summary>
        /// 部件故障列表
        /// </summary>
        public List<CompFaultItem> CompFaultList { get; set; }
        /// <summary>
        /// 部件卡片位置
        /// </summary>
        public List<double> CardPosition { get; set; }
        /// <summary>
        /// 测点位置
        /// </summary>
        public List<double> SpotPosition { get; set; }
    }


    public class CompFaultItem
    {
        public DateTime AcqTime { get; set; }
        public string? CreationName { get; set; }
        public string? Credibility { get; set; }
        public string? DiagnoseTime { get; set; }
        public string? EntityId { get; set; }

        public string? EntityName { get; set; }
        public string? EntityType { get; set; }
        public string? EvGroup { get; set; }
        public string? FaultCode { get; set; }
        public string? FaultId { get; set; }

        public string? FaultStatusName { get; set; }
        public string? FaultValue { get; set; }
        public string? ModelType { get; set; }
        public string? Remark { get; set; }
        public string? Severity { get; set; }

        public string? WindparkId { get; set; }
        public string? WindturbineId { get; set; }
    }
}

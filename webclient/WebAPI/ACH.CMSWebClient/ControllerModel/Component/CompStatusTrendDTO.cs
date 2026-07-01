namespace ACH.CMSWebClient.ControllerModel.Component
{
    /// <summary>
    /// 部件页面：部件状态趋势
    /// </summary>
    public class CompStatusTrendDTO
    {
        /// <summary>
        /// 部件ID
        /// </summary>
        public string? EntityId { get; set; } = "";
        /// <summary>
        /// 部件名称
        /// </summary>
        public string? EntityName { get; set; } = "";
        /// <summary>
        /// 部件类型
        /// </summary>
        public string? EntityType { get; set; } = "";
        /// <summary>
        /// 部件最新状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 部件最新状态时间
        /// </summary>
        public string StatusTime { get; set; }
        /// <summary>
        /// 时间范围内的部件状态
        /// </summary>
        public CompStatusTrendItemDTO EntityStatus { get; set; }

        // 构造函数
        public CompStatusTrendDTO(string entityId, string entityName, string entityType, string status, string statusTime, CompStatusTrendItemDTO entityStatus)
        {
            EntityId = entityId;
            EntityName = entityName;
            EntityType = entityType;
            Status = status;
            StatusTime = statusTime;
            EntityStatus = entityStatus;
        }
    }

    public class CompStatusTrendItemDTO
    {
        /// <summary>
        /// 时间列表
        /// </summary>
        public List<string> Time { get; set; }
        /// <summary>
        /// 状态列表
        /// </summary>
        public List<string> Status { get; set; }
    }
}

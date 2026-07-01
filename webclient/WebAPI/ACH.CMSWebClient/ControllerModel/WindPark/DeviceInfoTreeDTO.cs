using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationInfoTreeDTO : ISortable
    {
        /// <summary>
        /// 类型：表示是风场还是机组
        /// </summary>
        public string type { get; set; } = "windpark";
        /// <summary>
        /// 风场Code = 风场ID
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 风场ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 是否有地图资源
        /// </summary>
        public Boolean isImageryLayer { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<DeviceInfoTreeDTO> childNode { get; set; }

        public string GetSortableName() => name ?? string.Empty;
    }

    public class DeviceInfoTreeDTO : ISortable
    {
        /// <summary>
        /// 父节点ID = 风场ID
        /// </summary>
        public string deptCode { get; set; }
        /// <summary>
        /// 机组编号
        /// </summary>
        public string entityCode { get; set; }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string entityId { get; set; }
        /// <summary>
        /// 机组名称
        /// </summary>
        public string entityName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int entityNumbering { get; set; } = -1;
        /// <summary>
        /// 设备类型
        /// </summary>
        public string entityType { get; set; } = "WINDTURBINE";
        /// <summary>
        /// 设备状态    
        /// </summary>
        public string healthStatus { get; set; }

        public string GetSortableName() => entityName ?? string.Empty;
    }
}

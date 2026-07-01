using ACH.Helper.Comparer;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationCompStatusDTO : ISortable
    {
        /// <summary>
        /// 
        /// </summary>
        public string? ChangeCount { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string? ChangeRate { get; set; } = "";
        /// <summary>
        /// 部件Code
        /// </summary>
        public string? CompCode { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string? CompName { get; set; }
        /// <summary>
        /// 部件总数
        /// </summary>
        public int CompTotal { get; set; }
        /// <summary>
        /// 故障部件数
        /// </summary>
        public int FaultCompCount { get; set; }
        /// <summary>
        /// 部件状态列表
        /// </summary>
        public List<StationCompStatusNumDTO> FaultStatusList { get; set; }

        public string GetSortableName() => CompName ?? string.Empty;
    }

    public class StationCompStatusNumDTO
    {
        /// <summary>
        /// 该状态下的部件数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 该状态名称
        /// </summary>
        public string? Name { get; set; }
    }
}

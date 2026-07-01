
using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 专项分析传参
    /// </summary>
    public class GroupTrendFromBodyDTO
    {
        public List<GroupTrendAttribute> GTCAttributes { get; set; }
        /// <summary>
        /// 特征值分析类型
        /// </summary>
        public string GTCType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class GroupTrendAttribute
        {
            public string WindturbineID { get; set; }
            public string MeasLoctionID { get; set; }
        }
    }


    /// <summary>
    /// 专项分析出参
    /// </summary>
    public class GroupTrendChartDTO : ISortable
    {
        /// <summary>
        /// 测点Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 特征值名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 该特征值Code下的所有特征值ID列表
        /// </summary>
        public List<string> Ids { get; set; }
        /// <summary>
        /// 接口全部返回0
        /// </summary>
        public int CBFNumber { get; set; }
        /// <summary>
        /// 接口全部返回null
        /// </summary>
        public List<GroupTrendChartDTO> Children { get; set; }

        public string GetSortableName() => Name ?? string.Empty;
    }
}
